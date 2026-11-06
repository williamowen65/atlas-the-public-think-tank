"""Prevent rehearsal configuration from selecting main resources or Terraform state."""
import importlib.util
from pathlib import Path
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('deployment_targets', ROOT / 'infrastructure/scripts/check-deployment-target.py')
targets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(targets)


class DeploymentTargetTests(unittest.TestCase):
    def settings(self, installation='rehearsal', **overrides):
        prefix = 'atlas-rehearsal-example' if installation == 'rehearsal' else 'atlas-existing'
        values = dict(ATLAS_INSTALLATION=installation, GITHUB_REF='refs/heads/topic',
                      GITHUB_RUN_ATTEMPT='1', ATLAS_NAME_PREFIX=prefix,
                      AZURE_TFSTATE_KEY=f'rehearsal/{prefix}.tfstate',
                      ATLAS_VARIANT='demo', ATLAS_TARGET_APP=f'{prefix}-demo-ops',
                      AZURE_RESOURCE_GROUP=f'{prefix}-rg')
        if installation == 'main':
            values.update(GITHUB_REF='refs/heads/main', AZURE_TFSTATE_KEY='existing-state.tfstate')
        values.update(overrides)
        return values

    def test_main_requires_main_branch_but_rehearsal_accepts_feature_branch(self):
        targets.validate('selection', self.settings())
        targets.validate('selection', self.settings('main'))
        for values in [self.settings('main', GITHUB_REF='refs/heads/topic'),
                       self.settings(ATLAS_INSTALLATION='typo'),
                       self.settings(GITHUB_REF='refs/tags/v1')]:
            with self.subTest(values=values), self.assertRaises(ValueError):
                targets.validate('selection', values)

    def test_main_can_keep_existing_names_and_state(self):
        targets.validate('infrastructure', self.settings('main'))
        targets.validate('admin', self.settings('main'))
        targets.validate('website', self.settings('main', ATLAS_TARGET_APP='atlas-existing-demo'))

    def test_state_and_prefix_namespaces_cannot_be_crossed(self):
        targets.validate('infrastructure', self.settings())
        cases = [self.settings(AZURE_TFSTATE_KEY='existing-state.tfstate'),
                 self.settings(ATLAS_NAME_PREFIX='atlas-existing'),
                 self.settings(AZURE_TFSTATE_KEY='rehearsal/other.tfstate'),
                 self.settings('main', AZURE_TFSTATE_KEY='rehearsal/anything.tfstate'),
                 self.settings('main', ATLAS_NAME_PREFIX='atlas-rehearsal-example')]
        for values in cases:
            with self.subTest(values=values), self.assertRaises(ValueError):
                targets.validate('infrastructure', values)

    def test_fresh_apply_required_for_either_installation(self):
        for installation in ('main', 'rehearsal'):
            values = self.settings(installation, ATLAS_REQUIRE_FRESH_RUN='true')
            targets.validate('infrastructure', values)
            values['GITHUB_RUN_ATTEMPT'] = '2'
            with self.assertRaises(ValueError):
                targets.validate('infrastructure', values)

    def test_rehearsal_apps_cannot_point_to_main_or_wrong_variant_or_role(self):
        for variant in ('live', 'demo'):
            for mode, suffix in [('admin', '-ops'), ('website', '')]:
                app = f'atlas-rehearsal-example-{variant}{suffix}'
                values = self.settings(ATLAS_VARIANT=variant, ATLAS_TARGET_APP=app)
                targets.validate(mode, values)
                for wrong in [f'atlas-existing-{variant}{suffix}',
                              f'atlas-rehearsal-example-other{suffix}',
                              f'atlas-rehearsal-example-{variant}{"" if suffix else "-ops"}']:
                    with self.subTest(mode=mode, wrong=wrong), self.assertRaises(ValueError):
                        targets.validate(mode, dict(values, ATLAS_TARGET_APP=wrong))
        with self.assertRaises(ValueError):
            targets.validate('admin', self.settings(AZURE_RESOURCE_GROUP='atlas-existing-rg'))

    def test_every_cloud_workflow_gates_credentials_behind_explicit_selection(self):
        # The initial selection check has no Environment or Azure credentials.
        # Remaining jobs depend on it directly or through build/package/plan.
        for name, first_job in [('AtlasAzurePlan.yml', 'azure-plan'),
                                ('AtlasAzureApply.yml', 'azure-plan'),
                                ('AtlasAdmin.yml', 'package'), ('AtlasDeploy.yml', 'build')]:
            workflow = yaml.load((ROOT / '.github/workflows' / name).read_text(), Loader=yaml.BaseLoader)
            with self.subTest(workflow=name):
                choice = workflow['on']['workflow_dispatch']['inputs']['installation']
                self.assertEqual(choice['options'], ['rehearsal', 'main'])
                self.assertEqual(choice['default'], 'rehearsal')
                jobs = workflow['jobs']
                self.assertNotIn('environment', jobs['validate-target'])
                self.assertEqual(jobs[first_job]['needs'], 'validate-target')
                for job in jobs.values():
                    if 'environment' in job:
                        self.assertIn("inputs.installation == 'rehearsal'", job['environment'])
                        self.assertIn('atlas-rehearsal-', job['environment'])
                        commands = '\n'.join(s.get('run', '') for s in job['steps'])
                        self.assertIn('check-deployment-target.py', commands)


if __name__ == '__main__':
    unittest.main()
