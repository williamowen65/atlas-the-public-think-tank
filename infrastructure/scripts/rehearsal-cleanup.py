"""Prepare/validate a rehearsal-only Terraform destroy plan; never execute deletion.

Azure state/plan JSON comes through stdin, never a log or artifact. Every managed
resource must belong to the exact rehearsal resource group/subscription. Temporary
SQL overrides exist only in the cleanup runner; normal deployment keeps protection.
"""
import importlib.util
import json
import os
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location('deployment_target', ROOT / 'scripts/check-deployment-target.py')
targets = importlib.util.module_from_spec(spec)
spec.loader.exec_module(targets)

SQL_OVERRIDE = {'resource': {'azapi_resource': {'variant': {'lifecycle': {'prevent_destroy': False}}}}}
PROVIDER_OVERRIDE = {'provider': {'azurerm': {'features': [{'resource_group': [{'prevent_deletion_if_contains_resources': True}]}]}}}


def selection(settings: dict[str, str]) -> dict[str, str]:
    if settings.get('GITHUB_REF') != 'refs/heads/main' or settings.get('GITHUB_RUN_ATTEMPT') != '1':
        raise ValueError('Cleanup requires a fresh manual run from main.')
    operation = settings.get('CLEANUP_OPERATION')
    if operation not in ('plan', 'delete'):
        raise ValueError('Choose plan or delete.')
    if settings.get('ATLAS_REHEARSAL_NAME', '').strip().lower() == 'main':
        raise ValueError('Main is not a cleanup target; enter an existing rehearsal name.')
    if not settings.get('ATLAS_REHEARSAL_NAME', '').strip():
        raise ValueError('Enter an explicit rehearsal name; cleanup has no default target.')
    # Force rehearsal: there is deliberately no main-installation input.
    result = targets.validate('selection', dict(settings, ATLAS_INSTALLATION='rehearsal'))
    if operation == 'delete' and settings.get('CLEANUP_CONFIRMATION') != result['resource_group']:
        raise ValueError('Deletion requires the exact full rehearsal resource-group name as confirmation.')
    return result


def scope(settings: dict[str, str]) -> tuple[dict[str, str], str]:
    result = selection(settings)
    subscription = settings.get('ARM_SUBSCRIPTION_ID', '')
    if not re.fullmatch(r'[0-9a-fA-F-]{36}', subscription):
        raise ValueError('Expected the configured Azure subscription GUID.')
    if settings.get('TF_BACKEND_KEY') != result['state_key']:
        raise ValueError('Cleanup state key must match the selected rehearsal exactly.')
    if not settings.get('TF_BACKEND_RG') or settings['TF_BACKEND_RG'].lower() == result['resource_group'].lower():
        raise ValueError('Shared Terraform state storage must be outside the rehearsal group.')
    group_id = f'/subscriptions/{subscription}/resourceGroups/{result["resource_group"]}'.lower()
    return result, group_id


def resources(module: dict):
    for resource in module.get('resources', []):
        if resource.get('mode') not in ('managed', 'data'):
            raise ValueError('State/plan contains an unrecognized resource mode.')
        if resource['mode'] == 'managed':
            yield resource
    for child in module.get('child_modules', []):
        yield from resources(child)


def require_id(resource_id: str, group_id: str) -> None:
    if not isinstance(resource_id, str):
        raise ValueError('A managed resource is missing a verifiable Azure resource ID.')
    normalized = resource_id.rstrip('/').lower()
    if normalized != group_id and not normalized.startswith(group_id + '/providers/'):
        raise ValueError('Cleanup rejected: a resource is outside the selected rehearsal group/subscription.')


def check_state(state: dict, settings: dict[str, str]) -> int:
    result, group_id = scope(settings)
    managed = list(resources(state.get('values', {}).get('root_module', {})))
    if not managed:
        return 0  # An already-empty state is a harmless no-op, not an Azure group delete.
    group_found = False
    for resource in managed:
        values = resource.get('values', {})
        require_id(values.get('id'), group_id)
        if resource.get('type') == 'azurerm_resource_group':
            if values.get('id', '').rstrip('/').lower() != group_id:
                raise ValueError('The managed resource-group ID must equal the selected rehearsal group.')
            if values.get('name', '').lower() != result['resource_group'].lower():
                raise ValueError('State resource-group name does not match the selected rehearsal.')
            group_found = True
    if not group_found:
        raise ValueError('State has no matching managed rehearsal resource group; refusing partial/ambiguous cleanup.')
    return len(managed)


def check_plan(plan: dict, settings: dict[str, str]) -> int:
    _, group_id = scope(settings)
    if plan.get('errored'):
        raise ValueError('Cannot delete using an errored Terraform plan.')
    changes = plan.get('resource_changes', [])
    if not isinstance(changes, list):
        raise ValueError('Invalid Terraform resource change list.')
    check_state(plan.get('prior_state', {}), settings)
    deletions = 0
    for resource in changes:
        if resource.get('mode') == 'data':
            continue
        if resource.get('mode') != 'managed':
            raise ValueError('Destroy plan contains an unrecognized resource mode.')
        change = resource.get('change', {})
        actions = change.get('actions')
        if actions not in (['delete'], ['no-op']):
            raise ValueError('Cleanup accepts only pure deletion/no-op plans; creation, update and replacement are forbidden.')
        require_id((change.get('before') or {}).get('id'), group_id)
        if actions == ['delete']:
            deletions += 1
    if deletions and not list(resources(plan.get('prior_state', {}).get('values', {}).get('root_module', {}))):
        raise ValueError('A deletion plan must include the verified prior rehearsal state.')
    if list(resources(plan.get('planned_values', {}).get('root_module', {}))):
        raise ValueError('Cleanup plan still has planned managed resources; expected complete installation removal.')
    return deletions


def prepare(settings: dict[str, str], terraform_root: Path = ROOT / 'terraform') -> None:
    scope(settings)
    # Never edit the checked-in protection. Use Terraform's explicit override-file
    # mechanism for this guarded cleanup checkout and remove the files afterward.
    sql_module = terraform_root / 'modules/sql'
    source = (sql_module / 'main.tf').read_text()
    if not re.search(r'lifecycle\s*\{\s*prevent_destroy\s*=\s*true\s*\}', source):
        raise ValueError('Expected the normal SQL lifecycle protection before creating cleanup overrides.')
    for path, value in [(sql_module / 'rehearsal_cleanup_override.tf.json', SQL_OVERRIDE),
                        (terraform_root / 'rehearsal_cleanup_override.tf.json', PROVIDER_OVERRIDE)]:
        if path.exists():
            raise ValueError('Cleanup override already exists; use a fresh checkout.')
        path.write_text(json.dumps(value, indent=2) + '\n')


if __name__ == '__main__':
    try:
        mode = sys.argv[1] if len(sys.argv) == 2 else ''
        settings = dict(os.environ)
        target = selection(settings)
        if mode == 'select':
            if os.environ.get('GITHUB_OUTPUT'):
                with open(os.environ['GITHUB_OUTPUT'], 'a', encoding='utf-8') as output:
                    for key in ('name_prefix', 'resource_group', 'state_key'):
                        output.write(f'{key}={target[key]}\n')
            print(f'Cleanup target: {target["resource_group"]}; state: {target["state_key"]}.')
        elif mode == 'prepare':
            prepare(settings)
            print('Temporary rehearsal cleanup overrides prepared; tracked SQL protection is unchanged.')
        elif mode in ('state', 'plan'):
            data = json.load(sys.stdin)
            count = check_state(data, settings) if mode == 'state' else check_plan(data, settings)
            if mode == 'plan' and os.environ.get('GITHUB_OUTPUT'):
                with open(os.environ['GITHUB_OUTPUT'], 'a', encoding='utf-8') as output:
                    output.write(f'has_changes={str(count > 0).lower()}\n')
            message = f'Cleanup {mode} verified for {target["resource_group"]}: {count} managed resources.'
            print(message)
            if os.environ.get('GITHUB_STEP_SUMMARY'):
                with open(os.environ['GITHUB_STEP_SUMMARY'], 'a', encoding='utf-8') as output:
                    output.write(message + '\n\n')
        else:
            raise ValueError('Expected select, prepare, state or plan.')
    except (ValueError, OSError, KeyError, TypeError) as error:
        print(f'Rehearsal cleanup stopped: {error}', file=sys.stderr)
        raise SystemExit(1)
