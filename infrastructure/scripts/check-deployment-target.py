"""Validate an explicit installation and its protected deployment configuration.

No Azure calls or credentials are printed. Rehearsal has a reserved resource/state
namespace so a copied main target fails before Terraform or deployment starts.
Azure RBAC and GitHub Environment protection must also enforce the boundary.
"""
import os
import re
import sys


def validate(mode: str, settings: dict[str, str]) -> None:
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
    if installation == 'main' and branch != 'refs/heads/main':
        raise ValueError('The main installation can only be targeted from the main branch.')
    if settings.get('ATLAS_REQUIRE_FRESH_RUN') == 'true' and settings.get('GITHUB_RUN_ATTEMPT') != '1':
        raise ValueError('Start a fresh Terraform apply workflow instead of rerunning an old plan.')
    if mode == 'selection':
        return

    def prefix_namespace(prefix: str) -> None:
        if not re.fullmatch(r'[a-z][a-z0-9-]{4,25}[a-z0-9]', prefix):
            raise ValueError('Use a valid 6–27 character Atlas naming prefix.')
        rehearsal = prefix.startswith('atlas-rehearsal-')
        if rehearsal != (installation == 'rehearsal'):
            raise ValueError('Rehearsal requires an atlas-rehearsal- prefix; main must use another prefix.')

    if mode == 'infrastructure':
        prefix = required('ATLAS_NAME_PREFIX')
        prefix_namespace(prefix)
        key = required('AZURE_TFSTATE_KEY')
        if installation == 'rehearsal':
            if key != f'rehearsal/{prefix}.tfstate':
                raise ValueError('Rehearsal state key must be rehearsal/<ATLAS_NAME_PREFIX>.tfstate.')
        elif key.startswith('rehearsal/'):
            raise ValueError('The main installation cannot use a rehearsal state key.')
    elif mode in ('admin', 'website'):
        variant = required('ATLAS_VARIANT')
        if variant not in ('live', 'demo'):
            raise ValueError('Choose live or demo.')
        app = required('ATLAS_TARGET_APP')
        suffix = f'-{variant}-ops' if mode == 'admin' else f'-{variant}'
        if not app.endswith(suffix):
            raise ValueError('App name must match the selected variant and host role.')
        prefix = app[:-len(suffix)]
        prefix_namespace(prefix)
        if mode == 'admin' and required('AZURE_RESOURCE_GROUP') != f'{prefix}-rg':
            raise ValueError('Admin app and resource group must belong to the same installation.')
    else:
        raise ValueError('Unknown deployment target validation mode.')


if __name__ == '__main__':
    try:
        validate(sys.argv[1] if len(sys.argv) == 2 else '', dict(os.environ))
    except ValueError as error:
        print(f'Deployment target rejected: {error}', file=sys.stderr)
        raise SystemExit(1)
    print('Deployment selection and target configuration verified.')
