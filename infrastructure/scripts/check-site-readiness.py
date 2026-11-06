"""One bounded HTTP check; separate successful publication from site readiness.

Deploy mode reports pending verification without failing a successful publication.
Verify-only mode fails if readiness is not confirmed. Neither mode changes Azure,
runs SQL, waits for minutes, or retries the requests.
"""
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile


def probe(url: str, require_health_json: bool) -> tuple[bool, str]:
    with tempfile.TemporaryDirectory() as directory:
        body = Path(directory) / 'health.json'
        try:
            result = subprocess.run([
                'curl', '--silent', '--show-error', '--connect-timeout', '4',
                '--max-time', '8', '--output', str(body) if require_health_json else os.devnull,
                '--write-out', '%{http_code}', url,
            ], check=False, capture_output=True, text=True, timeout=10)
        except (OSError, subprocess.TimeoutExpired):
            return False, 'request did not complete'
        if result.returncode:
            return False, f'request failed (curl exit {result.returncode})'
        status = result.stdout.strip()
        if status != '200':
            return False, f'HTTP {status}'
        if require_health_json:
            try:
                payload = json.loads(body.read_text())
                if not isinstance(payload, dict) or payload.get('status') != 'ok':
                    return False, 'HTTP 200, unexpected liveness response'
            except (ValueError, OSError):
                return False, 'HTTP 200, unexpected liveness response'
        return True, 'HTTP 200'


def check(app_name: str, operation: str, request=probe) -> tuple[int, str]:
    if operation not in ('deploy', 'verify'):
        raise ValueError('Choose deploy or verify.')
    if not re.fullmatch(r'[a-z][a-z0-9-]{1,58}[a-z0-9]', app_name):
        raise ValueError('Expected a resolved Azure App Service name.')
    url = f'https://{app_name}.azurewebsites.net'
    live, live_detail = request(url + '/health/live', True)
    home, home_detail = request(url + '/', False)
    ready = live and home
    title = 'Readiness confirmed' if ready else 'Verification pending'
    lines = [f'### {title}', f'URL: {url}', '',
             f'- Atlas liveness: {live_detail}', f'- Home page: {home_detail}', '']
    if ready:
        lines.append('HTTP liveness and home page checks passed. SQL, authentication and release identity are not checked here.')
    elif operation == 'deploy':
        lines.extend(['Package publication succeeded; site readiness has not been confirmed.',
                      'Run this same Action later with operation **verify**, using the same installation, rehearsal name and variant.',
                      'If verification still fails, inspect the public app startup logs; do not assume it only needs more time.'])
    else:
        lines.extend(['Readiness was not confirmed. No build or deployment was performed.',
                      'Inspect the public app startup logs if the site remains unavailable.'])
    return (0 if ready or operation == 'deploy' else 1), '\n'.join(lines) + '\n'


def main() -> int:
    try:
        operation = os.environ.get('ATLAS_DEPLOY_OPERATION', '')
        code, summary = check(os.environ.get('APP_NAME', ''), operation)
    except ValueError as error:
        print(str(error), file=sys.stderr)
        return 1
    print(summary)
    if os.environ.get('GITHUB_STEP_SUMMARY'):
        with open(os.environ['GITHUB_STEP_SUMMARY'], 'a', encoding='utf-8') as output:
            output.write(summary)
    if '### Verification pending' in summary:
        print('::warning::Site readiness is unconfirmed. Run operation verify later; see the summary.')
    return code


if __name__ == '__main__':
    raise SystemExit(main())
