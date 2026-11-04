# Atlas infrastructure

The root of Atlas infrastructure as code (IaC). Runtime application adapters remain in Atlas.Application.

Start with the [Azure resource inventory](PROVISIONED-RESOURCES.md) for a detailed explanation of what Terraform declares and what deployment code still configures. Read [the deployment runbook](DEPLOYMENT.md) for commands and environment parity, [the security guide](SECURITY.md) for firewall/CI controls, [private network operations](PRIVATE-NETWORK-OPERATIONS.md) for migrations and privileged jobs, and [the telemetry guide](TELEMETRY.md) for Azure Monitor integration.

Terraform is organized under [terraform](terraform/) with reusable SQL, App Service and observability modules. The original configuration is under [legacy](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) for reference.

GitHub workflows validate infrastructure and deploy the API bundled with the frontend. Azure resource provisioning and cloud deployment are separately operator-triggered; neither runs automatically after a commit.
