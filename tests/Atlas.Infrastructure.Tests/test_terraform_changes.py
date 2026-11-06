"""Deletion checks inspect Terraform JSON without contacting Azure or printing values."""
import importlib.util
import json
import os
import tempfile

import yaml
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[2]
SCRIPT = ROOT / 'infrastructure/scripts/check-terraform-changes.py'
spec = importlib.util.spec_from_file_location('terraform_changes', SCRIPT)
changes = importlib.util.module_from_spec(spec)
spec.loader.exec_module(changes)


class TerraformChangeTests(unittest.TestCase):
    def test_variant_reduction_stops_before_terraform_prevent_destroy_error(self):
        state = {'values': {'root_module': {'child_modules': [
            {'resources': [{'mode': 'managed', 'address': 'module.sites["live"].azurerm_linux_web_app.atlas'}]},
            {'resources': [{'mode': 'managed', 'address': 'module.sql.azapi_resource.variant["demo"]'}]},
        ]}}}
        changes.check_existing_variants(state, ['live', 'demo'])
        with self.assertRaisesRegex(ValueError, 'already contains demo'):
            changes.check_existing_variants(state, ['live'])
        with self.assertRaisesRegex(ValueError, 'already contains live'):
            changes.check_existing_variants(state, ['demo'])
        changes.check_existing_variants({}, ['demo'])  # New state is allowed.

    def test_create_update_and_noop_allowed_but_delete_and_both_replacement_orders_blocked(self):
        for actions in [['create'], ['update'], ['no-op'], ['read']]:
            changes.check_plan({'resource_changes': [{'address': 'fixture', 'change': {'actions': actions}}]})
        for actions in [['delete'], ['delete', 'create'], ['create', 'delete']]:
            with self.subTest(actions=actions), self.assertRaisesRegex(ValueError, 'deletion or replacement'):
                changes.check_plan({'resource_changes': [{'address': 'fixture', 'change': {'actions': actions}}]})

    def test_failure_does_not_expose_plan_values(self):
        payload = {'resource_changes': [{'address': 'fixture', 'change': {
            'actions': ['delete'], 'before': {'password': 'fixture-private-password'}}}]}
        result = subprocess.run(['python', str(SCRIPT), 'plan'], input=json.dumps(payload),
                                capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn('fixture-private-password', result.stdout + result.stderr)
        self.assertIn('separate deliberate cleanup', result.stderr)

    def test_invalid_json_stops(self):
        result = subprocess.run(['python', str(SCRIPT), 'plan'], input='not json', capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)

    def test_preview_script_blocks_destructive_plans_and_cleans_temporary_files(self):
        # Run the real preview shell step using a stand-in Terraform command. This
        # verifies exit-code handling, the JSON pipe, and cleanup on failure too.
        workflow = yaml.load((ROOT / '.github/workflows/AtlasAzurePlan.yml').read_text(), Loader=yaml.BaseLoader)
        script = next(s['run'] for s in workflow['jobs']['azure-plan']['steps']
                      if s.get('name') == 'Review proposed Azure changes — NEVER apply')
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            cli = directory / 'terraform'
            cli.write_text('#!/bin/bash\n'
                'if [[ "$1" == "plan" ]]; then\n'
                '  for arg in "$@"; do if [[ "$arg" == -out=* ]]; then touch "${arg#-out=}"; fi; done\n'
                '  exit 2\n'
                'elif [[ "$1" == "show" ]]; then cat "$FIXTURE_PLAN"; else exit 99; fi\n')
            cli.chmod(0o755)
            fixture = directory / 'fixture.json'
            for actions, expected in [(['create'], 0), (['delete'], 1), (['create', 'delete'], 1)]:
                fixture.write_text(json.dumps({'resource_changes': [{'address': 'fixture', 'change': {
                    'actions': actions, 'before': {'password': 'fixture-private-password'}}}]}))
                result = subprocess.run(['bash', '-c', script], cwd=ROOT / 'infrastructure/terraform',
                    env=dict(os.environ, PATH=f'{directory}:{os.environ["PATH"]}',
                             RUNNER_TEMP=str(directory), FIXTURE_PLAN=str(fixture)),
                    capture_output=True, text=True)
                with self.subTest(actions=actions):
                    self.assertEqual(result.returncode, expected, result.stderr)
                    self.assertFalse((directory / 'atlas-preview.tfplan').exists())
                    self.assertNotIn('fixture-private-password', result.stdout + result.stderr)


if __name__ == '__main__':
    unittest.main()
