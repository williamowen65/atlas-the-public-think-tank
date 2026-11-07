# Atlas infrastructure

Start with the guide that matches your task.

## Deploy or maintain an installation

| Guide | Use it for |
| --- | --- |
| [Deployment overview](DEPLOYMENT.md) | Workflow diagram, Action controls and responsibilities |
| [Azure setup](AZURE-SETUP.md) | One-time account setup; where secrets, variables and IDs come from |
| [First deployment](FIRST-DEPLOYMENT.md) | The single step-by-step provisioning and deployment walkthrough |
| [Rehearsal cleanup](REHEARSAL-CLEANUP.md) | Plan and delete a test installation through Terraform |

## Understand the design

| Guide | Use it for |
| --- | --- |
| [Private network operations](PRIVATE-NETWORK-OPERATIONS.md) | Why SQL commands run through operations App Service SSH |
| [Resource inventory](PROVISIONED-RESOURCES.md) | Azure resource counts and Terraform ownership |
| [Security](SECURITY.md) | Access boundaries, secrets, approvals and deletion guards |
| [Telemetry](TELEMETRY.md) | Check Azure logs, requests and metrics; understand audit-delivery limits |

## Code and checks

- [Terraform modules](terraform/) declare the Azure resources.
- [Target profiles](config/deployment-targets.json) track public installation names
  and state keys.
- [Atlas.Admin](../src/hosts/Atlas.Admin/README.md) is the C# database tool.
- [Infrastructure tests](../tests/Atlas.Infrastructure.Tests/README.md) use Python
  `unittest` for configuration and script checks, rather than a .NET test project.

Cloud provisioning and publication are manually triggered through GitHub Actions.
A commit alone does not deploy Azure resources.

The [legacy Terraform](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf)
is retained for reference.
