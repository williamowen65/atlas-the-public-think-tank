"""Register Container Apps and delegate the Atlas worker's storage grant."""
import argparse
import json
import re
import subprocess
from uuid import UUID

RBAC_ADMIN = "f58310d9-a9f6-439a-9e8d-f62e7b41a168"
BLOB_CONTRIBUTOR = "ba92f5b4-2d11-453d-a403-e96b0029c9fe"


def azure(*args):
    result = subprocess.run(["az", *args, "--only-show-errors", "--output", "json"],
                            check=True, capture_output=True, text=True)
    return json.loads(result.stdout) if result.stdout.strip() else None


def worker_condition(principal_id):
    rules = []
    for action, source in (("write", "Request"), ("delete", "Resource")):
        rules.append(f"""(
    (!(ActionMatches{{'Microsoft.Authorization/roleAssignments/{action}'}}))
    OR (
        @{source}[Microsoft.Authorization/roleAssignments:RoleDefinitionId] ForAnyOfAnyValues:GuidEquals {{{BLOB_CONTRIBUTOR}}}
        AND @{source}[Microsoft.Authorization/roleAssignments:PrincipalId] ForAnyOfAnyValues:GuidEquals {{{principal_id}}}
    )
)""")
    return " AND ".join(rules)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--subscription", required=True, type=UUID)
    parser.add_argument("--resource-group", required=True)
    parser.add_argument("--storage-account", required=True)
    parser.add_argument("--worker-identity", required=True)
    parser.add_argument("--apply-principal", required=True, type=UUID)
    args = parser.parse_args()

    subscription = ("--subscription", str(args.subscription))
    principal_id = UUID(azure("identity", "show", *subscription, "--resource-group",
                             args.resource_group, "--name", args.worker_identity)["principalId"])
    account = azure("storage", "account", "show", *subscription, "--resource-group",
                    args.resource_group, "--name", args.storage_account)
    scope = account["id"] + "/blobServices/default/containers/share-images"
    condition = worker_condition(principal_id)

    azure("provider", "register", *subscription, "--namespace", "Microsoft.App", "--wait")
    assignments = azure("role", "assignment", "list", *subscription, "--scope", scope,
                        "--assignee-object-id", str(args.apply_principal), "--role", RBAC_ADMIN,
                        "--all")
    for assignment in assignments:
        if assignment["scope"].lower() != scope.lower():
            continue
        existing = re.sub(r"\s+", "", assignment.get("condition") or "")
        if existing == re.sub(r"\s+", "", condition) and assignment.get("conditionVersion") == "2.0":
            print("Worker delegation already configured.")
            return
        raise SystemExit("A different RBAC Administrator assignment already exists at this exact scope. Review it before continuing.")

    azure("role", "assignment", "create", *subscription, "--scope", scope,
          "--assignee-object-id", str(args.apply_principal), "--assignee-principal-type",
          "ServicePrincipal", "--role", RBAC_ADMIN, "--condition", condition,
          "--condition-version", "2.0")
    print(f"Configured worker delegation at {scope}.")


if __name__ == "__main__":
    try:
        main()
    except subprocess.CalledProcessError as error:
        raise SystemExit(error.stderr.strip() or f"Azure CLI failed ({error.returncode}).")
