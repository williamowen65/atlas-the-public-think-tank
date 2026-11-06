# Atlas infrastructure

The root of Atlas infrastructure as code (IaC). Runtime application adapters remain in Atlas.Application.

For a fresh clone or fork, start with [Azure and GitHub setup](AZURE-SETUP.md) to find the required IDs and configure secrets, variables, OIDC and roles.

For reviewed rehearsal deletion and state reconciliation, see [Rehearsal cleanup](REHEARSAL-CLEANUP.md).

Cloud Actions share the existing `atlas-infra-apply` credential Environment. Public installation targets are tracked in [deployment-targets.json](config/deployment-targets.json) and selected in the Action dropdown.

For the working operator sequence and a second isolated installation, start with the [first-deployment walkthrough](FIRST-DEPLOYMENT.md).

See the [Azure resource inventory](PROVISIONED-RESOURCES.md) for a detailed explanation of what Terraform declares and what deployment code still configures.

Read [the deployment runbook](DEPLOYMENT.md) for commands and environment parity, [the security guide](SECURITY.md) for firewall/CI controls, [private network operations](PRIVATE-NETWORK-OPERATIONS.md) for migrations and privileged jobs, and [the telemetry guide](TELEMETRY.md) for Azure Monitor integration.

Terraform is organized under [terraform](terraform/) with reusable SQL, App Service and observability modules. The original configuration is under [legacy](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) for reference.

The architecture and security policy tests are under [root tests/Atlas.Infrastructure.Tests](../tests/Atlas.Infrastructure.Tests/README.md), alongside Atlas's other test suites. They remain Python `unittest` checks rather than a .NET test project because they parse HCL declarations directly.

GitHub workflows validate infrastructure and deploy the API bundled with the frontend. Azure resource provisioning and cloud deployment are separately operator-triggered; neither runs automatically after a commit.
