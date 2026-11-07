"""Exercise admin packaging and shell guards offline, without Azure or SQL access."""
import os
import shutil
from pathlib import Path
import subprocess
import tempfile
import unittest
import zipfile

import yaml

ROOT = Path(__file__).resolve().parents[2]
# BaseLoader keeps GitHub's `on` key as text rather than YAML 1.1's boolean.
WORKFLOW = yaml.load((ROOT / '.github/workflows/AtlasAdmin.yml').read_text(), Loader=yaml.BaseLoader)


def step(job, name):
    return next(s for s in WORKFLOW['jobs'][job]['steps'] if s.get('name') == name)


class AdminWorkflowTests(unittest.TestCase):
    def run_script(self, script, directory, **settings):
        env = dict(os.environ, **settings)
        return subprocess.run(['bash', '-c', script], cwd=directory, env=env,
                              capture_output=True, text=True)

    @unittest.skipUnless(shutil.which('dotnet') and
        (ROOT / 'src/hosts/Atlas.Admin/bin/Release/net10.0/Atlas.Admin.dll').exists(),
        'Requires .NET 10 and a Release build of Atlas.Admin')
    def test_moderator_cli_refuses_missing_and_cross_environment_confirmation(self):
        dll = ROOT / 'src/hosts/Atlas.Admin/bin/Release/net10.0/Atlas.Admin.dll'
        self.assertTrue(dll.exists(), 'Build Atlas.Admin in Release before running this test')
        env = dict(os.environ,
            ATLAS_DEPLOYMENT_VARIANT='demo', ATLAS_EXPECTED_DATABASE='fixture-demo',
            ATLAS_ADMIN_MI_CLIENT_ID='11111111-1111-1111-1111-111111111111',
            ATLAS_SQL_CONNECTION_STRING='Server=fixture.database.windows.net,1433;Database=fixture-demo;'
                'Authentication=Active Directory Managed Identity;'
                'User Id=11111111-1111-1111-1111-111111111111;Encrypt=True;TrustServerCertificate=False;')
        for arguments, code, message in [
            (['--grant-global-moderator'], 2, 'Moderator:'),
            (['--grant-global-moderator', 'account@example.test'], 2, 'Moderator:'),
            (['--grant-global-moderator', '', 'grant-demo'], 2, 'Moderator:'),
            (['--grant-global-moderator', 'account@example.test', 'grant-live'], 1,
                'confirmation must match'),
        ]:
            with self.subTest(arguments=arguments):
                result = subprocess.run(['dotnet', str(dll), *arguments], env=env,
                    capture_output=True, text=True, timeout=20)
                self.assertEqual(result.returncode, code, result.stderr)
                self.assertIn(message, result.stderr)

    def test_operation_confirmation_before_deployment(self):
        # A mistyped or cross-variant bootstrap confirmation must fail before
        # credentials are used; publication never needs bootstrap confirmation.
        script = step('execute', 'Verify explicit operation and target')['run']
        for variant, operation, confirmation, succeeds in [
            ('live', 'publish', '', True), ('demo', 'publish', '', True),
            ('live', 'bootstrap', 'bootstrap-live', True),
            ('demo', 'bootstrap', 'bootstrap-demo', True),
            ('live', 'bootstrap', 'bootstrap-demo', False),
            ('demo', 'bootstrap', '', False), ('other', 'publish', '', False),
            ('live', 'schema', '', False), ('demo', 'seed-demo', '', False),
            ('live', 'diagnose', '', False),
        ]:
            with self.subTest(variant=variant, operation=operation, confirmation=confirmation):
                result = self.run_script(script, ROOT, VARIANT=variant,
                                         OPERATION=operation, CONFIRMATION=confirmation)
                self.assertEqual(result.returncode == 0, succeeds, result.stderr)

    def test_package_has_ssh_tools_and_only_bootstrap_job(self):
        # Use a stand-in build output to test the real packaging script. No .NET
        # executable runs here; CI separately compiles the actual admin project.
        with tempfile.TemporaryDirectory() as directory:
            build = Path(directory) / 'atlas-admin-publish'
            build.mkdir()
            (build / 'Atlas.Admin.dll').write_text('test fixture')
            result = self.run_script(step('package', 'Package SSH tools and the identity bootstrap job')['run'],
                                     ROOT, RUNNER_TEMP=directory)
            self.assertEqual(result.returncode, 0, result.stderr)
            with zipfile.ZipFile(Path(directory) / 'atlas-admin-tools.zip') as archive:
                names = archive.namelist()
                self.assertIn('tools/atlas-admin/Atlas.Admin.dll', names)
                self.assertIn('App_Data/jobs/triggered/atlas-bootstrap/run.sh', names)
                jobs = {n.split('/')[3] for n in names if n.startswith('App_Data/jobs/triggered/') and len(n.split('/')) > 4}
                self.assertEqual(jobs, {'atlas-bootstrap'})
                self.assertFalse(any('Atlas.Console' in n for n in names))

    def test_publish_checks_variant_cleans_old_jobs_and_never_triggers_sql(self):
        # Replace Azure CLI with a recorder: inspect only the commands the actual
        # publish step would issue, including refusal of the wrong target variant.
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            cli = directory / 'az'
            cli.write_text('#!/bin/bash\nprintf "%s\\n" "$*" >> "$CALL_LOG"\n'
                           'if [[ "$1 $2 $3" == "webapp config appsettings" ]]; then echo "$ACTUAL_VARIANT"; fi\n')
            cli.chmod(0o755)
            for actual, app, expected_success in [('live', 'fixture-live-ops', True),
                                                  ('demo', 'fixture-live-ops', False),
                                                  ('live', 'fixture-live', False)]:
                log = directory / 'calls'
                log.write_text('')
                result = self.run_script(step('execute', 'Publish admin tools')['run'], ROOT,
                    PATH=f'{directory}:{os.environ["PATH"]}', CALL_LOG=str(log),
                    ACTUAL_VARIANT=actual, VARIANT='live', ADMIN_APP=app,
                    RESOURCE_GROUP='fixture-rg', GITHUB_STEP_SUMMARY=str(directory / 'summary'))
                self.assertEqual(result.returncode == 0, expected_success, result.stderr)
                calls = log.read_text()
                self.assertNotIn('webjob triggered run', calls)
                self.assertEqual('webapp deploy' in calls, expected_success)
                if expected_success:
                    self.assertIn('--clean true', calls)
                    self.assertIn('atlas-admin-tools.zip', calls)

    def test_bootstrap_is_the_only_conditional_trigger(self):
        trigger = step('execute', 'Bootstrap SQL identities and verify completion')
        self.assertEqual(trigger['if'], "${{ inputs.operation == 'bootstrap' }}")
        self.assertEqual(trigger['env']['ATLAS_ADMIN_JOB'], 'atlas-bootstrap')
        self.assertIn('wait-for-webjob.py', trigger['run'])

    def test_bootstrap_entrypoint_requires_credentials(self):
        # Run the packaged entrypoint in isolation, substituting dotnet with an
        # argument recorder. Missing credentials must prevent any command launch.
        with tempfile.TemporaryDirectory() as directory:
            directory = Path(directory)
            script = directory / 'run.sh'
            script.write_text((ROOT / 'infrastructure/webjobs/run-webjob.sh').read_text())
            dotnet = directory / 'dotnet'
            dotnet.write_text('#!/bin/bash\nprintf "%s\\n" "$*" > "$CALL_LOG"\n')
            dotnet.chmod(0o755)
            env = dict(PATH=f'{directory}:{os.environ["PATH"]}', CALL_LOG=str(directory / 'calls'),
                       ATLAS_DEPLOYMENT_VARIANT='demo', ATLAS_EXPECTED_DATABASE='fixture-demo',
                       ATLAS_SQL_CONNECTION_STRING='Database=fixture-demo;Authentication=Active Directory Managed Identity;',
                       ATLAS_BOOTSTRAP_SQL_USERNAME='fixture', ATLAS_BOOTSTRAP_SQL_PASSWORD='fixture')
            result = self.run_script('bash ./run.sh', directory, **env)
            self.assertEqual(result.returncode, 0, result.stderr)
            self.assertEqual((directory / 'calls').read_text().strip(), './Atlas.Admin.dll --bootstrap-cloud-identities')
            (directory / 'calls').unlink()
            env['ATLAS_BOOTSTRAP_SQL_PASSWORD'] = ''
            result = self.run_script('bash ./run.sh', directory, **env)
            self.assertNotEqual(result.returncode, 0)
            self.assertFalse((directory / 'calls').exists())


if __name__ == '__main__':
    unittest.main()
