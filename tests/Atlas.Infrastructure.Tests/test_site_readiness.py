"""Publication and readiness must have distinct results, without long retry loops."""
import importlib.util
from pathlib import Path
from types import SimpleNamespace
import unittest
from unittest.mock import patch

import yaml

ROOT = Path(__file__).resolve().parents[2]
spec = importlib.util.spec_from_file_location('readiness', ROOT / 'infrastructure/scripts/check-site-readiness.py')
readiness = importlib.util.module_from_spec(spec)
spec.loader.exec_module(readiness)


class SiteReadinessTests(unittest.TestCase):
    def test_pending_after_publish_does_not_claim_readiness_or_fail_publication(self):
        calls = []
        def request(url, health):
            calls.append(url)
            return (False, 'HTTP 404') if health else (True, 'HTTP 200')
        code, summary = readiness.check('atlas-rehearsal-fixture-live', 'deploy', request)
        self.assertEqual(code, 0)
        self.assertIn('Verification pending', summary)
        self.assertIn('site readiness has not been confirmed', summary)
        self.assertIn('operation **verify**', summary)
        self.assertEqual(len(calls), 2)  # One request per endpoint; no retries.

    def test_verify_only_fails_if_either_endpoint_is_unavailable(self):
        for health_ready, root_ready in [(False, True), (True, False), (False, False)]:
            def request(url, health):
                ready = health_ready if health else root_ready
                return ready, 'HTTP 200' if ready else 'HTTP 503'
            code, summary = readiness.check('atlas-rehearsal-fixture-live', 'verify', request)
            self.assertEqual(code, 1)
            self.assertIn('No build or deployment was performed', summary)

    def test_success_is_explicit_and_does_not_claim_sql_or_release_verification(self):
        for operation in ['deploy', 'verify']:
            code, summary = readiness.check('atlas-rehearsal-fixture-live', operation,
                                             lambda url, health: (True, 'HTTP 200'))
            self.assertEqual(code, 0)
            self.assertIn('Readiness confirmed', summary)
            self.assertIn('SQL, authentication and release identity are not checked', summary)

    def test_http_200_placeholder_does_not_pass_atlas_liveness(self):
        for body, expected in [('<html>Azure placeholder</html>', False),
                               ('{"status":"ok"}', True), ('{"status":"starting"}', False)]:
            def curl(command, **kwargs):
                self.assertEqual(command[command.index('--max-time') + 1], '8')
                self.assertEqual(kwargs['timeout'], 10)
                Path(command[command.index('--output') + 1]).write_text(body)
                return SimpleNamespace(returncode=0, stdout='200')
            with patch.object(readiness.subprocess, 'run', side_effect=curl):
                actual, _ = readiness.probe('https://fixture.azurewebsites.net/health/live', True)
                self.assertEqual(actual, expected)

    def test_bad_configuration_fails_instead_of_reporting_successful_deployment(self):
        for app, operation in [('bad/name', 'deploy'), ('atlas-fixture', 'typo')]:
            with self.assertRaises(ValueError):
                readiness.check(app, operation)

    def test_verification_skips_build_and_has_no_azure_credentials(self):
        workflow = yaml.load((ROOT / '.github/workflows/AtlasDeploy.yml').read_text(), Loader=yaml.BaseLoader)
        self.assertEqual(workflow['on']['workflow_dispatch']['inputs']['operation']['options'], ['deploy', 'verify'])
        jobs = workflow['jobs']
        self.assertEqual(jobs['build']['if'], "inputs.operation == 'deploy'")
        self.assertIn('build', jobs['deploy']['needs'])
        self.assertNotIn('environment', jobs['verify'])
        self.assertNotIn('id-token', jobs['verify']['permissions'])
        self.assertIn("inputs.operation == 'verify'", jobs['verify']['if'])
        self.assertIn("needs.deploy.result == 'success'", jobs['verify']['if'])
        for step in jobs['verify']['steps']:
            self.assertNotIn('azure/login', step.get('uses', ''))
        self.assertNotIn('Smoke test public HTTPS liveness and home page', str(jobs['deploy']))


if __name__ == '__main__':
    unittest.main()
