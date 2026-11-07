# Azure resource inventory

This page describes what the application Terraform root creates.
Use the selected installation’s current plan and outputs for its actual resources.

## Hosting and data

| Resource | Both live and demo | One variant |
| --- | ---: | ---: |
| B1 App Service plan | 1 | 1 |
| Public Web Apps | 2 | 1 |
| Operations Web Apps | 2 | 1 |
| User-assigned managed identities | 4 | 2 |
| SQL logical server | 1 | 1 |
| SQL databases | 2 | 1 |

Public apps serve the API and frontend together. Operations apps share the B1 plan
but use separate identities for database administration.

For application choices and defaults, see [Deployment overview](DEPLOYMENT.md#choose-the-target).

## Private networking

These counts are the same for either installation shape.

| Resource | Count | Purpose |
| --- | ---: | --- |
| VNet | 1 | Shared private network |
| Subnets | 2 | App integration and private endpoint |
| SQL private endpoint | 1 | Private access to the shared SQL server |
| Private DNS zone and VNet link | 1 each | SQL hostname resolution |
| NSG and subnet association | 1 each | Restrict endpoint-subnet traffic |

## Monitoring and grouping

| Resource | Count per installation | Purpose |
| --- | ---: | --- |
| Application Insights | 1 | Shared application monitoring |
| Log Analytics workspace | 1 | Telemetry storage; configured for 30-day retention |
| Resource group | 1 | Groups the application resources |

Verify actual monitoring ingestion using [Telemetry](TELEMETRY.md).
Creating the resources alone does not prove data is arriving.

## Where the configuration lives

| File or module | Owns |
| --- | --- |
| [Root](terraform/main.tf) | Resource group, shared plan, identities and module composition |
| [SQL](terraform/modules/sql/main.tf) | Private logical server and databases |
| [Network](terraform/modules/network/main.tf) | VNet, subnets, endpoint, DNS and NSG |
| [Public apps](terraform/modules/webapp/main.tf) | API/frontend hosting and runtime settings |
| [Operations apps](terraform/modules/admin/main.tf) | Administration hosting and bootstrap settings |
| [Observability](terraform/modules/observability/main.tf) | Application Insights and Log Analytics |
| [Variables](terraform/variables.tf) | Inputs, SKUs and credentials |
| [Outputs](terraform/outputs.tf) | URLs, resource names and identity client IDs |

## Outside this Terraform root

| Item | Handling |
| --- | --- |
| State storage, GitHub OIDC and Environment protection | One-time [Azure setup](AZURE-SETUP.md) |
| SQL users, application schema and seed data | Explicit steps in [First deployment](FIRST-DEPLOYMENT.md) |
| Image-upload Blob Storage | Not provisioned; image-storage work is deferred |
| Secret vault and automatic credential rotation | Not provisioned; runtime SQL uses managed identity |
| Custom domain, extra backup policies, alerts and cost budgets | Add explicitly when needed |
| A/B assignment, variant-switching UI and read-only demo behavior | Product features, not infrastructure created by this root |

The Terraform state-storage account is not an image-upload store.

## Costs and operating limits

The SQL module explicitly requests the serverless free offer. Ineligible regions or
subscriptions should fail provisioning rather than silently select paid Basic.

Free compute, data and backup allowances have limits. Terraform does not enforce an
overall spending cap. See [deployment costs](DEPLOYMENT.md#costs-and-verification).

For runtime boundaries, use [Private network operations](PRIVATE-NETWORK-OPERATIONS.md).
For controls and monitoring checks, use [Security](SECURITY.md) and
[Telemetry](TELEMETRY.md).
