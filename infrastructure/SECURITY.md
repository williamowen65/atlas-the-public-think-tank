# Azure deployment security

This page describes the deployment’s controls and their limits.
Successful provisioning is not a production security audit.

## Access to Azure

Cloud workflows use short-lived GitHub OIDC tokens and the shared
`atlas-infra-apply` Environment.

- Only reviewed, trusted branches should have access.
- Main installation runs require branch `main`.
- Required reviewers protect credential-bearing jobs.
- Azure role grants and state-storage access must be configured separately.

Both main and rehearsals use the same deployment identity. Naming checks prevent
targeting mistakes; they do not restrict malicious code holding those credentials.

See [Azure setup](AZURE-SETUP.md) for OIDC trust, roles and value sources.

## Application and database boundaries

| Control | Purpose |
| --- | --- |
| HTTPS-only apps; TLS 1.2 minimum | Encrypt public traffic |
| FTP publishing disabled | Close an unused publication path |
| SQL public networking disabled | Keep SQL reachable through Private Link |
| VNet, private DNS and endpoint NSG | Limit the network path to SQL |
| Separate runtime and operations identities | Keep schema privileges off the public app |
| Separate database per variant | Keep live and demo data distinct |

Bootstrap establishes the contained SQL users and grants. Verify those grants in
Azure, including rejection of cross-database access.

See [Private network operations](PRIVATE-NETWORK-OPERATIONS.md) for the connection
diagram and [Resource inventory](PROVISIONED-RESOURCES.md) for Terraform locations.

## Secrets and Terraform state

Temporary SQL administrator settings belong only on operations hosts during
bootstrap. Remove them afterward using the walkthrough.

Keep these private:

- SQL administrator and demo-account passwords.
- Terraform state and its retained versions.
- Plaintext Terraform plans, tokens and connection strings.

Marking a Terraform value sensitive hides ordinary display; it does not remove
the value from state. Removing bootstrap app settings does not erase state history.

Normal runtime SQL sign-in uses managed identity rather than a SQL password.

## Plan and apply protections

The preview Action reads the real remote state but never applies its plan.
It uses the same privileged identity as apply: it is read-only by behavior,
not through a separate read-only Azure role.

The apply Action:

1. Rejects removal of existing variants and plans containing deletion/replacement.
2. Encrypts its saved plan for transfer.
3. Requires recorded Environment approval.
4. Checks the decrypted plan and applies that exact plan.

Start a fresh run instead of retrying an old saved-plan job.
Preview plans are temporary and not uploaded; raw state/plan JSON is not printed.

## Rehearsal deletion protections

Normal SQL configuration retains `prevent_destroy = true`.
Only the dedicated cleanup workflow temporarily overrides it after checking scope.

Cleanup requires:

- Branch `main` and an explicit rehearsal name.
- Existing state with managed resources in the exact subscription/resource group.
- A deletion-only plan.
- The full resource-group name as confirmation and a recorded approval.

The provider also requires an empty resource group before removing it. Untracked
resources can therefore block final group deletion rather than being silently removed.

These guards do not prevent deletion through Azure Portal or privileged SQL writes.
Use [Rehearsal cleanup](REHEARSAL-CLEANUP.md) for the operator steps.

## Automated checks

[Terraform validation](../.github/workflows/AtlasInfrastructure.yml) does not deploy
or authenticate to Azure.

| Check | Result |
| --- | --- |
| Terraform format and validation | Blocking configuration checks |
| Python infrastructure tests | Blocking policy and script/workflow checks |
| Trivy secret scan | Blocking high/critical secret findings |
| Trivy misconfiguration scan | Advisory high/critical findings |

Offline checks cannot see deployed drift, actual SQL permissions or runtime behavior.
Run instructions and test coverage live in the
[infrastructure test guide](../tests/Atlas.Infrastructure.Tests/README.md).

## Checks still needed in the deployed system

### Permissions and sessions

- Confirm runtime cannot administer schema or access the other variant’s database.
- Check sign-in/out, CSRF, API authorization and rejected requests.
- Review deployed settings and Portal-side drift.

### Recovery and operations

- Exercise backup/restore and review secret rotation.
- Review scan findings, logs, retention, alerts and spending.
- Verify rate limiting when using multiple application instances.

### Monitoring and audit

Application diagnostics and durable audit delivery are different.
Use the [telemetry checklist](TELEMETRY.md) to verify ingestion and review the
remaining cloud audit-delivery work.
