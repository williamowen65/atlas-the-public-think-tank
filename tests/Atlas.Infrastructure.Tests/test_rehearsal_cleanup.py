"""Rehearsal cleanup must reject main, unrelated IDs and non-destroy plans."""
import importlib.util
import json
import os
import shutil
from pathlib import Path
import subprocess
import tempfile
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / 'infrastructure/scripts/rehearsal-cleanup.py'
spec = importlib.util.spec_from_file_location('cleanup', SCRIPT)
cleanup = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cleanup)
SUBSCRIPTION = '11111111-2222-3333-4444-555555555555'
GROUP = 'atlas-rehearsal-trial02-rg'
GROUP_ID = f'/subscriptions/{SUBSCRIPTION}/resourceGroups/{GROUP}'


class RehearsalCleanupTests(unittest.TestCase):
    def settings(self, **overrides):
        settings = dict(GITHUB_REF='refs/heads/main', GITHUB_RUN_ATTEMPT='1',
                        CLEANUP_OPERATION='delete', CLEANUP_CONFIRMATION=GROUP,
                        ATLAS_REHEARSAL_NAME='trial02', ARM_SUBSCRIPTION_ID=SUBSCRIPTION,
                        TF_BACKEND_RG='atlas-tfstate-rg',
                        TF_BACKEND_KEY='rehearsal/atlas-rehearsal-trial02.tfstate')
        settings.update(overrides)
        return settings

    def state(self):
        return {'values': {'root_module': {'resources': [
            {'mode': 'managed', 'type': 'azurerm_resource_group', 'values': {'id': GROUP_ID, 'name': GROUP}},
        ], 'child_modules': [{'resources': [
            {'mode': 'managed', 'type': 'azapi_resource', 'values': {
                'id': GROUP_ID + '/providers/Microsoft.Sql/servers/fixture/databases/fixture-live'}},
        ]}]}}}

    def plan(self):
        state = self.state()
        return {'prior_state': state, 'planned_values': {}, 'resource_changes': [
            {'mode': 'managed', 'change': {'actions': ['delete'], 'before': resource['values']}}
            for resource in cleanup.resources(state['values']['root_module'])]}

    def test_exact_confirmation_and_main_branch_required_before_credentials(self):
        result = cleanup.selection(self.settings())
        self.assertEqual(result['resource_group'], GROUP)
        self.assertEqual(result['state_key'], 'rehearsal/atlas-rehearsal-trial02.tfstate')
        for overrides in [dict(GITHUB_REF='refs/heads/topic'), dict(GITHUB_RUN_ATTEMPT='2'),
                          dict(ATLAS_REHEARSAL_NAME=''), dict(ATLAS_REHEARSAL_NAME='main'), dict(CLEANUP_CONFIRMATION='delete'),
                          dict(CLEANUP_CONFIRMATION='atlas-7k4m9q2x-rg'), dict(CLEANUP_OPERATION='apply')]:
            with self.subTest(overrides=overrides), self.assertRaises(ValueError):
                cleanup.selection(self.settings(**overrides))
        cleanup.selection(self.settings(CLEANUP_OPERATION='plan', CLEANUP_CONFIRMATION=''))

    def test_main_state_and_shared_backend_group_rejected(self):
        for overrides in [dict(TF_BACKEND_KEY='atlas-capstone.tfstate'),
                          dict(TF_BACKEND_RG=GROUP), dict(TF_BACKEND_RG='')]:
            with self.subTest(overrides=overrides), self.assertRaises(ValueError):
                cleanup.scope(self.settings(**overrides))

    def test_all_managed_resources_must_belong_to_the_exact_group_and_subscription(self):
        self.assertEqual(cleanup.check_state(self.state(), self.settings()), 2)
        for wrong in [GROUP_ID.replace(GROUP, 'atlas-7k4m9q2x-rg'),
                      GROUP_ID.replace(SUBSCRIPTION, 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee'),
                      GROUP_ID + '-other/providers/Microsoft.Sql/servers/fixture', None]:
            state = self.state()
            state['values']['root_module']['child_modules'][0]['resources'][0]['values']['id'] = wrong
            with self.subTest(wrong=wrong), self.assertRaises(ValueError):
                cleanup.check_state(state, self.settings())
        state = self.state()
        state['values']['root_module']['resources'] = []
        with self.assertRaisesRegex(ValueError, 'no matching managed rehearsal resource group'):
            cleanup.check_state(state, self.settings())
        self.assertEqual(cleanup.check_state({}, self.settings()), 0)

    def test_destroy_plan_must_not_create_update_or_replace_anything(self):
        self.assertEqual(cleanup.check_plan(self.plan(), self.settings()), 2)
        for actions in [['create'], ['update'], ['delete', 'create'], ['create', 'delete']]:
            plan = self.plan()
            plan['resource_changes'][0]['change']['actions'] = actions
            with self.subTest(actions=actions), self.assertRaises(ValueError):
                cleanup.check_plan(plan, self.settings())

    def test_recheck_plan_blocks_main_ids_even_if_prior_state_was_rehearsal(self):
        plan = self.plan()
        plan['resource_changes'][0]['change']['before']['id'] = GROUP_ID.replace(GROUP, 'atlas-7k4m9q2x-rg')
        with self.assertRaises(ValueError):
            cleanup.check_plan(plan, self.settings())
        plan = self.plan()
        plan['planned_values'] = self.state()['values']
        with self.assertRaises(ValueError):
            cleanup.check_plan(plan, self.settings())

    def test_sql_override_is_temporary_and_normal_source_stays_protected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            sql = root / 'modules/sql'
            sql.mkdir(parents=True)
            original = 'resource "azapi_resource" "variant" { lifecycle { prevent_destroy = true } }\n'
            (sql / 'main.tf').write_text(original)
            cleanup.prepare(self.settings(), root)
            self.assertEqual((sql / 'main.tf').read_text(), original)
            generated = json.loads((sql / 'rehearsal_cleanup_override.tf.json').read_text())
            self.assertEqual(generated, cleanup.SQL_OVERRIDE)
            provider = json.loads((root / 'rehearsal_cleanup_override.tf.json').read_text())
            self.assertTrue(provider['provider']['azurerm']['features'][0]['resource_group'][0]['prevent_deletion_if_contains_resources'])
            with self.assertRaises(ValueError):
                cleanup.prepare(self.settings(), root)
        self.assertIn('prevent_destroy = true', (ROOT / 'infrastructure/terraform/modules/sql/main.tf').read_text())

    def test_invalid_scope_does_not_print_secret_state_values(self):
        state = self.state()
        state['values']['root_module']['resources'][0]['values'].update(
            id='/subscriptions/main/resourceGroups/main', password='fixture-private-password')
        result = subprocess.run(['python', str(SCRIPT), 'state'], input=json.dumps(state),
            env=dict(os.environ, **self.settings()), capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn('fixture-private-password', result.stdout + result.stderr)

    def test_workflow_has_no_main_selector_or_unguarded_azure_group_delete(self):
        path = ROOT / '.github/workflows/AtlasAzureCleanup.yml'
        text = path.read_text()
        workflow = yaml.load(text, Loader=yaml.BaseLoader)
        inputs = workflow['on']['workflow_dispatch']['inputs']
        self.assertNotIn('installation', inputs)
        self.assertEqual(inputs['operation']['default'], 'plan')
        self.assertEqual(inputs['rehearsal_name']['required'], 'true')
        self.assertEqual(workflow['concurrency']['group'], 'atlas-azure-terraform-rehearsal')
        jobs = workflow['jobs']
        self.assertNotIn('environment', jobs['validate-target'])
        self.assertEqual(jobs['azure-plan']['environment'], 'atlas-infra-apply')
        self.assertIn("inputs.operation == 'delete'", jobs['delete']['if'])
        self.assertIn('Require a recorded environment approval', str(jobs['delete']))
        self.assertNotIn('vars.AZURE_TFSTATE_KEY', text)  # Never fall back to main state.
        self.assertNotIn('az group delete', text)
        self.assertIn('az storage blob exists', text)
        self.assertNotIn('az storage blob delete', text)
        delete_step = next(s for s in jobs['delete']['steps'] if s.get('name') == 'Apply the exact reviewed destroy plan')
        self.assertLess(delete_step['run'].index('rehearsal-cleanup.py plan'), delete_step['run'].index('terraform apply'))

    @unittest.skipUnless(shutil.which('terraform'), 'Terraform CLI is needed for the local-only lifecycle fixture')
    def test_terraform_honors_temporary_override_without_editing_protected_source(self):
        # terraform_data is built into Terraform: no cloud account/provider is used.
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = 'resource "terraform_data" "variant" {\n input = "fixture"\n lifecycle { prevent_destroy = true }\n}\n'
            (root / 'main.tf').write_text(source)
            for arguments in [['init', '-backend=false', '-input=false', '-no-color'],
                              ['apply', '-auto-approve', '-input=false', '-no-color']]:
                result = subprocess.run(['terraform', *arguments], cwd=root, capture_output=True, text=True)
                self.assertEqual(result.returncode, 0, result.stderr)
            command = ['terraform', 'plan', '-destroy', '-input=false', '-no-color']
            blocked = subprocess.run(command, cwd=root, capture_output=True, text=True)
            self.assertNotEqual(blocked.returncode, 0)
            self.assertIn('prevent_destroy', blocked.stderr)
            # Same override structure as the SQL cleanup, with the fixture's type.
            override = {'resource': {'terraform_data': cleanup.SQL_OVERRIDE['resource']['azapi_resource']}}
            (root / 'rehearsal_cleanup_override.tf.json').write_text(json.dumps(override))
            allowed = subprocess.run(command, cwd=root, capture_output=True, text=True)
            self.assertEqual(allowed.returncode, 0, allowed.stderr)
            self.assertEqual((root / 'main.tf').read_text(), source)


if __name__ == '__main__':
    unittest.main()
