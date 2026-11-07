"""Resolve public deployment profiles while keeping Azure credentials shared.

The single GitHub Environment supplies credentials/account settings; this file
validates tracked target profiles and emits only non-secret names as job outputs.
These checks prevent mistakes; they are not an Azure authorization boundary.
"""
import json
import os
from pathlib import Path
import re
import sys

CONFIG = Path(__file__).resolve().parents[1] / 'config/deployment-targets.json'


def validate(mode: str, settings: dict[str, str], profiles: dict | None = None) -> dict[str, str]:
    def required(name: str) -> str:
        value = settings.get(name, '')
        if not value:
            raise ValueError(f'Missing deployment configuration: {name}')
        return value

    installation = required('ATLAS_INSTALLATION')
    if installation not in ('main', 'rehearsal'):
        raise ValueError('Choose main or rehearsal explicitly.')
    branch = required('GITHUB_REF')
    if not branch.startswith('refs/heads/'):
        raise ValueError('Deployments must run from a branch, not a tag or PR merge ref.')
    # Website selection explicitly opts in to branch testing on demo only.
    # Infrastructure/admin callers do not receive this exception.
    demo_website = (mode == 'website' or (
        mode == 'selection' and settings.get('ATLAS_TARGET_PURPOSE') == 'website'
    )) and settings.get('ATLAS_VARIANT') == 'demo'
    if installation == 'main' and branch != 'refs/heads/main' and not demo_website:
        raise ValueError('Main live, infrastructure and admin operations require the main branch.')
    if settings.get('ATLAS_REQUIRE_FRESH_RUN') == 'true' and settings.get('GITHUB_RUN_ATTEMPT') != '1':
        raise ValueError('Start a fresh Terraform apply workflow instead of rerunning an old plan.')

    profiles = json.loads(CONFIG.read_text()) if profiles is None else profiles
    if set(profiles) != {'main', 'rehearsal'}:
        raise ValueError('Tracked profiles must match the main/rehearsal Action choices.')
    for name, profile in profiles.items():
        if not isinstance(profile, dict):
            raise ValueError('Each deployment profile must be an object.')
        prefix = profile.get('name_prefix', '')
        if not isinstance(prefix, str) or not re.fullmatch(r'[a-z][a-z0-9-]{4,25}[a-z0-9]', prefix):
            raise ValueError('Use a valid 6–27 character Atlas naming prefix.')
        if prefix.startswith('atlas-rehearsal-') != (name == 'rehearsal'):
            raise ValueError('Only rehearsal uses the reserved atlas-rehearsal- resource prefix.')
        key = profile.get('state_key')
        # Null only preserves the existing main key from the shared Environment;
        # the operator can copy that exact non-secret value into JSON later.
        if name == 'main' and key is None:
            continue
        if not isinstance(key, str) or not re.fullmatch(r'[A-Za-z0-9_./-]+\.tfstate', key):
            raise ValueError('Configure a valid state blob key ending in .tfstate.')
        if name == 'rehearsal' and key != f'rehearsal/{prefix}.tfstate':
            raise ValueError('Rehearsal state key must be rehearsal/<name_prefix>.tfstate.')
        if name == 'main' and key.startswith('rehearsal/'):
            raise ValueError('Main cannot use a rehearsal state key.')

    profile = profiles[installation]
    prefix = profile['name_prefix']
    state_key = profile.get('state_key') or ''
    requested_name = settings.get('ATLAS_REHEARSAL_NAME', '').strip()
    if requested_name:
        if installation != 'rehearsal':
            raise ValueError('A custom rehearsal name is only allowed for installation rehearsal.')
        prefix = requested_name if requested_name.startswith('atlas-rehearsal-') else 'atlas-rehearsal-' + requested_name
        if not re.fullmatch(r'atlas-rehearsal-[a-z0-9](?:[a-z0-9-]*[a-z0-9])?', prefix) or len(prefix) > 27:
            raise ValueError('Use a rehearsal suffix of lowercase letters/digits/hyphens; the full prefix must be at most 27 characters.')
        state_key = f'rehearsal/{prefix}.tfstate'
    shape = settings.get('ATLAS_DEPLOYMENT_SHAPE', 'default')
    if shape == 'default':
        shape = 'both' if installation == 'main' else 'live'
    if shape not in ('both', 'live', 'demo'):
        raise ValueError('Choose default, both, live or demo for the applications to provision.')
    variants = ['live', 'demo'] if shape == 'both' else [shape]
    result = dict(name_prefix=prefix, state_key=state_key,
                  variants_json=json.dumps(variants, separators=(',', ':')),
                  resource_group=f'{prefix}-rg', live_app=f'{prefix}-live',
                  demo_app=f'{prefix}-demo', live_admin=f'{prefix}-live-ops',
                  demo_admin=f'{prefix}-demo-ops')
    if mode == 'selection':
        return result
    if mode == 'infrastructure':
        if required('ATLAS_NAME_PREFIX') != prefix:
            raise ValueError('Terraform prefix does not match the selected tracked profile.')
        key = required('AZURE_TFSTATE_KEY')
        if result['state_key']:
            if key != result['state_key']:
                raise ValueError('Terraform state key does not match the selected tracked profile.')
        elif key.startswith('rehearsal/') or not re.fullmatch(r'[A-Za-z0-9_./-]+\.tfstate', key):
            raise ValueError('The existing main state key must be valid and outside rehearsal/.')
    elif mode in ('admin', 'website'):
        variant = required('ATLAS_VARIANT')
        if variant not in ('live', 'demo'):
            raise ValueError('Choose live or demo.')
        expected = result[f'{variant}_admin' if mode == 'admin' else f'{variant}_app']
        if required('ATLAS_TARGET_APP') != expected:
            raise ValueError('App name does not match the selected tracked installation and variant.')
        if mode == 'admin' and required('AZURE_RESOURCE_GROUP') != result['resource_group']:
            raise ValueError('Admin resource group does not match the selected tracked profile.')
    else:
        raise ValueError('Unknown deployment target validation mode.')
    return result


if __name__ == '__main__':
    try:
        mode = sys.argv[1] if len(sys.argv) == 2 else ''
        result = validate(mode, dict(os.environ))
        if mode == 'selection' and os.environ.get('GITHUB_OUTPUT'):
            with open(os.environ['GITHUB_OUTPUT'], 'a', encoding='utf-8') as output:
                for name, value in result.items():
                    output.write(f'{name}={value}\n')
    except (ValueError, OSError) as error:
        print(f'Deployment target rejected: {error}', file=sys.stderr)
        raise SystemExit(1)
    print(f'Deployment profile verified: {os.environ["ATLAS_INSTALLATION"]}; resource prefix {result["name_prefix"]}.')
