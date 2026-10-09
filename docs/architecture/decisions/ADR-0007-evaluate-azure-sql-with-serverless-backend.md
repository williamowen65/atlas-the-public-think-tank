# ADR-0007: Evaluate Azure SQL Database with a serverless backend

Status: Proposed  
Date: 2026-10-09  
Related decisions: [ADR-0004: SQL Server persistence](ADR-0004-use-sql-server-persistence.md), [ADR-0006: Identity mapping](ADR-0006-identity-participant-mapping.md)  
Research: [Cloud hosting research](../../deployment/CLOUD-HOSTING-RESEARCH.md)

## Context

Atlas needs an affordable initial cloud deployment and a defensible path to growth. Operating virtual machines is less desirable than deploying through managed hosting. Predictable costs matter, but no traffic measurements, monthly budget, latency targets, or regional deployment estimates have been established by this research.

The current rewrite is a .NET 10 modular application hosted by Atlas.Console with EF Core SQL Server persistence and ASP.NET Core Identity infrastructure. A new HTTP host and production delivery configuration are still needed. The domains do not need to become independently deployed services to use serverless hosting.

The project owner wants to consider Azure SQL with a serverless backend. Backend execution and database compute are separate decisions: a serverless backend can use either a provisioned or a serverless Azure SQL Database.

## Decision

Propose evaluating **Azure SQL Database with serverless application compute** as the primary deployment direction:

- Compare **Azure Functions Flex Consumption**, using .NET isolated worker HTTP handlers, with **Azure Container Apps Consumption**, using a conventional containerized ASP.NET Core API.
- Start the REST API hosting comparison with Container Apps because it preserves conventional ASP.NET Core hosting. This preference is an architectural inference; Functions remains a primary candidate.
- Evaluate **General Purpose serverless** and a suitable **provisioned Azure SQL Database tier** separately. Preserve the accepted SQL Server provider direction, subject to migrations and integration tests proving Azure SQL compatibility.
- Use **Azure App Service with Azure SQL Database** as the provisioned application-capacity baseline. Retain **Elastic Beanstalk with RDS for SQL Server** as an AWS comparison.
- Compare complete deployment costs, performance, and management effort using the linked research process. Keep the modular application together unless a later requirement justifies distributed deployment.

This is a proposed evaluation direction, not an accepted production provider, SKU, database tier, or commitment to provision resources. Regional quotes and representative experiments are pending.

## Consequences

- Atlas can explore serverless hosting without administering application or database VMs.
- The cloud bill must include database, storage, networking, monitoring, supporting services, and additional environments. Temporary credits cannot establish long-term affordability.
- Backend cold starts and database resume can combine to delay a request. Open SQL sessions or background activity may prevent database auto-pause; retries, connection usage, and idle behavior need verification.
- HTTP authentication/authorization, managed database access, migrations, Data Protection where required, durable event delivery, and recovery remain implementation work. Hosting selection does not complete them.
- Application rollback and database recovery must be evaluated together. Replica limits and budget alerts are cost controls, not a guaranteed cap on the total bill.

## Alternatives considered

- **Azure App Service:** managed web hosting with provisioned capacity; useful baseline when predictable response times or persistent usage outweigh scale-to-zero savings.
- **Elastic Beanstalk + RDS:** a managed application workflow that retains EC2-based resources and additional platform/network decisions.
- **Lightsail instances or containers:** simple capacity pricing; managed Lightsail databases do not offer SQL Server, so a separate database arrangement is needed.
- **Lambda or Fargate:** credible serverless function/container alternatives; retain for further comparison when their deployment model merits the integration work.
- **Self-managed application/database VMs:** more infrastructure work than the preferred initial direction.
- **Change database engine to reduce costs:** outside this proposal; requires revisiting ADR-0004 and assessing migration and verification effort.

## Evidence required before acceptance

Agree budget, latency, availability, and recovery targets; save dated regional calculator estimates and assumptions; measure representative Atlas workflows including idle/startup and SQL connections; verify migrations, Identity, authorization, and restore behavior; then record the selected backend, database tier, full costs, and reasons in this ADR. Update the research as evidence changes and revisit the decision when actual workload or operating needs materially change.
