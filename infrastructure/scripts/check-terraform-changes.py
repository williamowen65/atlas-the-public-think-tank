"""Read Terraform JSON privately and reject implicit removal/replacement.

Input is piped directly from `terraform show -json`; never echo raw state/plan
because it contains secrets. Diagnostics identify addresses/variant labels only.
"""
import json
import os
import re
import sys


def managed_resources(module: dict):
    yield from (r for r in module.get('resources', []) if r.get('mode') == 'managed')
    for child in module.get('child_modules', []):
        yield from managed_resources(child)


def check_existing_variants(state: dict, selected: list[str]) -> None:
    existing = set()
    for resource in managed_resources(state.get('values', {}).get('root_module', {})):
        address = resource.get('address', '')
        # These addresses are the variant-keyed resources from this Terraform root.
        matches = re.findall(r'(?:module\.(?:sites|admin_sites)|azurerm_user_assigned_identity\.(?:runtime|operations)|azapi_resource\.variant)\["(live|demo)"\]', address)
        existing.update(matches)
    removed = existing - set(selected)
    if removed:
        raise ValueError('This state already contains ' + ', '.join(sorted(removed)) +
                         '. The selected application set would remove it. Keep the existing set, '
                         'or choose a new rehearsal name/state. Deletion is a separate explicit operation.')


def check_plan(plan: dict, demo_only: bool = False) -> None:
    changes = plan.get('resource_changes', [])
    if not isinstance(changes, list):
        raise ValueError('Terraform plan did not contain a valid resource change list.')
    if demo_only:
        # Targeting includes dependencies. Fail closed if Terraform proposes any
        # mutation beyond the demo web app; never expose the planned values.
        forbidden = [r.get('address', '(unknown resource)') for r in changes
                     if r.get('mode', 'managed') == 'managed'
                     and r.get('change', {}).get('actions', []) not in (['no-op'], ['read'])
                     and not r.get('address', '').startswith('module.sites["demo"].')]
        if forbidden:
            raise ValueError('Demo-only plan would change live or shared resources: '
                             + ', '.join(forbidden) + '. Run shared infrastructure changes from main.')
    destructive = [r.get('address', '(unknown resource)') for r in changes
                   if 'delete' in r.get('change', {}).get('actions', [])]
    if destructive:
        raise ValueError('Plan stopped: resource deletion or replacement is not allowed in this deployment workflow: '
                         + ', '.join(destructive) +
                         '. Review the configuration or choose a new rehearsal name. '
                         'Use a separate deliberate cleanup operation for deletion.')


if __name__ == '__main__':
    try:
        mode = sys.argv[1] if len(sys.argv) == 2 else ''
        data = json.load(sys.stdin)
        if mode == 'state':
            selected = json.loads(os.environ['ATLAS_VARIANTS_JSON'])
            if not isinstance(selected, list) or not selected or not set(selected) <= {'live', 'demo'}:
                raise ValueError('Invalid selected application set.')
            check_existing_variants(data, selected)
        elif mode == 'plan':
            check_plan(data, demo_only=os.environ.get('ATLAS_TERRAFORM_DEMO_ONLY') == 'true')
        else:
            raise ValueError('Expected state or plan validation mode.')
    except (ValueError, KeyError, TypeError) as error:
        print(f'Terraform deployment stopped: {error}', file=sys.stderr)
        raise SystemExit(1)
    print('Terraform removal checks passed; no deletion or replacement selected.')
