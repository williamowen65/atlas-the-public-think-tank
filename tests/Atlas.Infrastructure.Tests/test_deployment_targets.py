"""Prevent rehearsal configuration from selecting main resources or Terraform state."""
import importlib.util
import copy
import json
import os
import subprocess
import tempfile
from pathlib import Path
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('deployment_targets', ROOT / 'infrastructure/scripts/check-deployment-target.py')
targets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(targets)


PROFILES = {
    'main': {'name_prefix': 'atlas-existing', 'state_key': None},
    'rehearsal': {'name_prefix': 'atlas-rehearsal-example', 'state_key': 'rehearsal/atlas-rehearsal-example.tfstate'},
}


class DeploymentTargetTests(unittest.TestCase):
    def validate(self, mode, settings):
        return targets.validate(mode, settings, PROFILES)

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
        self.validate('selection', self.settings())
        self.validate('selection', self.settings('main'))
        for values in [self.settings('main', GITHUB_REF='refs/heads/topic'),
                       self.settings(ATLAS_INSTALLATION='typo'),
                       self.settings(GITHUB_REF='refs/tags/v1')]:
            with self.subTest(values=values), self.assertRaises(ValueError):
                self.validate('selection', values)

    def test_main_demo_branch_operations_are_explicit_and_preserve_live_state(self):
        branch = self.settings('main', GITHUB_REF='refs/heads/topic')
        website = dict(branch, ATLAS_TARGET_PURPOSE='website', ATLAS_TARGET_APP='atlas-existing-demo')
        self.validate('selection', website)
        self.validate('website', website)
        for variant in ['live', '']:
            with self.assertRaises(ValueError):
                self.validate('selection', dict(website, ATLAS_VARIANT=variant))
        infra = dict(branch, ATLAS_TARGET_PURPOSE='infrastructure', ATLAS_DEPLOYMENT_SHAPE='demo')
        for mode in ['selection', 'infrastructure']:
            selected = self.validate(mode, infra)
            self.assertEqual(json.loads(selected['variants_json']), ['live', 'demo'])
            self.assertEqual(selected['terraform_demo_only'], 'true')
        for shape in ['live', 'both', 'default']:
            with self.assertRaises(ValueError):
                self.validate('selection', dict(infra, ATLAS_DEPLOYMENT_SHAPE=shape))
        with self.assertRaises(ValueError):
            self.validate('infrastructure', dict(infra, TF_VAR_enable_sql_bootstrap='true'))
        with self.assertRaises(ValueError):
            self.validate('admin', infra)

    def test_main_can_keep_existing_names_and_state(self):
        self.validate('infrastructure', self.settings('main'))
        self.validate('admin', self.settings('main'))
        self.validate('website', self.settings('main', ATLAS_TARGET_APP='atlas-existing-demo'))

    def test_state_and_prefix_namespaces_cannot_be_crossed(self):
        self.validate('infrastructure', self.settings())
        cases = [self.settings(AZURE_TFSTATE_KEY='existing-state.tfstate'),
                 self.settings(ATLAS_NAME_PREFIX='atlas-existing'),
                 self.settings(AZURE_TFSTATE_KEY='rehearsal/other.tfstate'),
                 self.settings('main', AZURE_TFSTATE_KEY='rehearsal/anything.tfstate'),
                 self.settings('main', ATLAS_NAME_PREFIX='atlas-rehearsal-example')]
        for values in cases:
            with self.subTest(values=values), self.assertRaises(ValueError):
                self.validate('infrastructure', values)

    def test_fresh_apply_required_for_either_installation(self):
        for installation in ('main', 'rehearsal'):
            values = self.settings(installation, ATLAS_REQUIRE_FRESH_RUN='true')
            self.validate('infrastructure', values)
            values['GITHUB_RUN_ATTEMPT'] = '2'
            with self.assertRaises(ValueError):
                self.validate('infrastructure', values)

    def test_rehearsal_apps_cannot_point_to_main_or_wrong_variant_or_role(self):
        for variant in ('live', 'demo'):
            for mode, suffix in [('admin', '-ops'), ('website', '')]:
                app = f'atlas-rehearsal-example-{variant}{suffix}'
                values = self.settings(ATLAS_VARIANT=variant, ATLAS_TARGET_APP=app)
                self.validate(mode, values)
                for wrong in [f'atlas-existing-{variant}{suffix}',
                              f'atlas-rehearsal-example-other{suffix}',
                              f'atlas-rehearsal-example-{variant}{"" if suffix else "-ops"}']:
                    with self.subTest(mode=mode, wrong=wrong), self.assertRaises(ValueError):
                        self.validate(mode, dict(values, ATLAS_TARGET_APP=wrong))
        with self.assertRaises(ValueError):
            self.validate('admin', self.settings(AZURE_RESOURCE_GROUP='atlas-existing-rg'))

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
                        self.assertEqual(job['environment'], 'atlas-infra-apply')
                        commands = '\n'.join(s.get('run', '') for s in job['steps'])
                        self.assertIn('check-deployment-target.py', commands)

    def test_profile_outputs_derive_all_apps_without_azure_or_secrets(self):
        result = self.validate('selection', self.settings())
        self.assertEqual(result['demo_admin'], 'atlas-rehearsal-example-demo-ops')
        self.assertEqual(result['live_app'], 'atlas-rehearsal-example-live')
        self.assertEqual(result['resource_group'], 'atlas-rehearsal-example-rg')
        self.assertEqual(result['state_key'], PROFILES['rehearsal']['state_key'])
        main = self.validate('selection', self.settings('main'))
        self.assertEqual(main['state_key'], '')  # Preserve the legacy shared Environment key.

    def test_invalid_profiles_cannot_inject_outputs_or_share_state(self):
        for change in [dict(name_prefix='atlas-existing'),
                       dict(state_key='existing-state.tfstate'),
                       dict(state_key='rehearsal/injected.tfstate\nname_prefix=atlas-existing')]:
            profiles = copy.deepcopy(PROFILES)
            profiles['rehearsal'].update(change)
            with self.subTest(change=change), self.assertRaises(ValueError):
                targets.validate('selection', self.settings(), profiles)
        profiles = copy.deepcopy(PROFILES)
        profiles['main']['state_key'] = 'rehearsal/atlas-rehearsal-example.tfstate'
        with self.assertRaises(ValueError):
            targets.validate('selection', self.settings('main'), profiles)

    def test_tracked_profiles_and_action_choices_match(self):
        profiles = json.loads(targets.CONFIG.read_text())
        self.assertEqual(set(profiles), {'main', 'rehearsal'})
        # Resolve the committed rehearsal profile, independently of the fixture.
        result = targets.validate('selection', self.settings())
        self.assertEqual(result['state_key'], f"rehearsal/{result['name_prefix']}.tfstate")
        for filename in ['AtlasAzureApply.yml', 'AtlasAzurePlan.yml', 'AtlasAdmin.yml', 'AtlasDeploy.yml']:
            text = (ROOT / '.github/workflows' / filename).read_text()
            for removed in ['vars.ATLAS_NAME_PREFIX', 'vars.AZURE_WEBAPP_NAME', 'vars.AZURE_ADMIN_WEBAPP_NAME', 'vars.AZURE_RESOURCE_GROUP']:
                self.assertNotIn(removed, text)
            workflow = yaml.load(text, Loader=yaml.BaseLoader)
            self.assertIn('demo_admin', workflow['jobs']['validate-target']['outputs'])

    def test_profile_cli_emits_nonsecret_outputs_for_downstream_jobs(self):
        # Exercise the same entrypoint used by the credential-free selection job.
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory) / 'outputs'
            result = subprocess.run(['python', str(ROOT / 'infrastructure/scripts/check-deployment-target.py'), 'selection'],
                env=dict(os.environ, ATLAS_INSTALLATION='rehearsal', GITHUB_REF='refs/heads/topic',
                         GITHUB_OUTPUT=str(output), TF_SQL_ADMIN_PASSWORD='fixture-private-value'),
                capture_output=True, text=True)
            self.assertEqual(result.returncode, 0, result.stderr)
            values = dict(line.split('=', 1) for line in output.read_text().splitlines())
            self.assertEqual(values['demo_app'], values['name_prefix'] + '-demo')
            self.assertNotIn('fixture-private-value', output.read_text() + result.stdout + result.stderr)
        for filename in ['AtlasAzureApply.yml', 'AtlasAzurePlan.yml', 'AtlasAdmin.yml', 'AtlasDeploy.yml']:
            workflow = yaml.load((ROOT / '.github/workflows' / filename).read_text(), Loader=yaml.BaseLoader)
            for job in workflow['jobs'].values():
                if 'needs.validate-target.outputs.' in json.dumps(job):
                    dependencies = job['needs']
                    if isinstance(dependencies, str):
                        dependencies = [dependencies]
                    self.assertIn('validate-target', dependencies)

    def test_main_and_rehearsal_shape_defaults_and_explicit_choices(self):
        self.assertEqual(json.loads(self.validate('selection', self.settings('main'))['variants_json']), ['live', 'demo'])
        self.assertEqual(json.loads(self.validate('selection', self.settings())['variants_json']), ['live'])
        for shape, expected in [('both', ['live', 'demo']), ('demo', ['demo']), ('live', ['live'])]:
            for installation in ('main', 'rehearsal'):
                result = self.validate('selection', self.settings(installation, ATLAS_DEPLOYMENT_SHAPE=shape))
                self.assertEqual(json.loads(result['variants_json']), expected)
        with self.assertRaises(ValueError):
            self.validate('selection', self.settings(ATLAS_DEPLOYMENT_SHAPE='none'))

    def test_custom_name_derives_consistent_state_and_targets_in_every_stage(self):
        settings = self.settings(ATLAS_REHEARSAL_NAME='trial02')
        result = self.validate('selection', settings)
        self.assertEqual(result['name_prefix'], 'atlas-rehearsal-trial02')
        self.assertEqual(result['state_key'], 'rehearsal/atlas-rehearsal-trial02.tfstate')
        # Shape deliberately does not enter the state key: changing shape on an
        # existing name is an update, handled by the removal check, not new state.
        other = self.validate('selection', dict(settings, ATLAS_DEPLOYMENT_SHAPE='both'))
        self.assertEqual(result['state_key'], other['state_key'])
        self.validate('infrastructure', dict(settings, ATLAS_NAME_PREFIX=result['name_prefix'], AZURE_TFSTATE_KEY=result['state_key']))
        self.validate('admin', dict(settings, ATLAS_TARGET_APP=result['demo_admin'], AZURE_RESOURCE_GROUP=result['resource_group']))
        self.validate('website', dict(settings, ATLAS_TARGET_APP=result['demo_app']))
        for name in ['../../existing', 'HasCaps', 'a' * 40, 'bad\nname']:
            with self.subTest(name=name), self.assertRaises(ValueError):
                self.validate('selection', self.settings(ATLAS_REHEARSAL_NAME=name))
        with self.assertRaises(ValueError):
            self.validate('selection', self.settings('main', ATLAS_REHEARSAL_NAME='trial02'))


if __name__ == '__main__':
    unittest.main()
