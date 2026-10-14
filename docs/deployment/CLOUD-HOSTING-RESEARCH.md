# Cloud hosting research for Atlas

Status: Initial research; regional estimates and deployment experiments pending  
Researched: 2026-10-09  
Decision record: [ADR-0007](../architecture/decisions/ADR-0007-evaluate-azure-sql-with-serverless-backend.md)

## Purpose and current position

Find an affordable way to deploy Atlas while keeping infrastructure management manageable and preserving a path to growth. The primary candidate is **Azure SQL Database with a serverless application backend**. Compare that candidate with a managed web application on provisioned capacity before choosing a production platform.

This document preserves the research, sources, assumptions, calculations, and unanswered questions. The ADR records the proposed direction and the evidence needed to accept it. Neither document claims that a cloud environment has been provisioned or that a lowest-cost configuration has been proved.

The repository baseline inspected was `main` at `2f899d7726524ae2baaa347c4e18c2b2231929d5`:

- [Atlas.Console](../../src/Atlas.Console/Atlas.Console.csproj) targets .NET 10 and composes the domain projects. A new HTTP host is still needed for the rewrite; the legacy web application is a separate implementation.
- [Atlas.Persistence](../../src/Atlas.Persistence/Atlas.Persistence.csproj) uses EF Core's SQL Server provider. [ADR-0004](../architecture/decisions/ADR-0004-use-sql-server-persistence.md) is the accepted persistence direction.
- [Identity](../architecture/IDENTITY.md) uses account infrastructure and Participant profiles. Cloud infrastructure authentication must be distinguished from Atlas user authentication.
- The [architecture guide](../architecture/README.md) describes a modular application and an in-memory event publisher with production delivery work still needed. Separate domains do not require separate cloud services.
- [Infrastructure](../../infrastructure/README.md) is currently an IaC placeholder. Traffic, resource consumption, budget, availability targets, and recovery targets have not been established by this research.

## Hosting responsibilities and billing are separate choices

There are more than two useful hosting options. First choose how much infrastructure Atlas should manage; then choose how capacity is billed.

| Model | What Atlas deploys and manages |
|---|---|
| Virtual machine / IaaS | Application plus operating-system configuration, patching, and server operations |
| Managed web platform / PaaS | Application and configuration; provider manages the hosting platform |
| Managed containers | Container image and configuration; provider manages compute hosts |
| Functions / FaaS | Triggered handlers and configuration; provider manages execution infrastructure |
| SaaS | A finished application used by its customers, rather than infrastructure for hosting Atlas |

Azure is a cloud provider, not one hosting model. **Azure App Service** is PaaS. **Elastic Beanstalk** provides a managed application platform but still provisions resources such as EC2 instances. **Lightsail instances** are simplified virtual servers; **Lightsail Containers** are a different managed container product. [S1, S2, S3]

Serverless means the application team does not administer the underlying servers. It can describe functions or containers, and it does not guarantee scale-to-zero or a particular price. A managed platform may reserve capacity while still removing server administration. A provisioned VM's compute bill can be predictable at a fixed size, but it also has a capacity limit and additional storage, networking, and operational costs.

## Azure SQL with a serverless backend

Use **Azure SQL Database** as the initial managed database candidate. It uses the SQL Server engine without requiring Atlas to maintain a database VM. Its logical server is a management and connection boundary, not a VM Atlas administers. Azure SQL Managed Instance and SQL Server on an Azure VM are separate products. [S4]

Azure SQL Database is a plausible fit for the existing EF Core provider, not an already-verified drop-in replacement. Check feature differences and run Atlas's migrations, relational-integrity tests, and Identity workflows against it. A change to PostgreSQL or MySQL would require a separate persistence decision. [S5]

Make two independent decisions:

1. **Backend compute:** Functions Flex Consumption or Container Apps Consumption.
2. **Database compute:** General Purpose serverless or a provisioned Azure SQL Database tier.

Either backend can use either database compute model. A serverless backend does not require a serverless database.

### Candidate A: Azure Functions Flex Consumption + Azure SQL Database

| Attribute | Research finding |
|---|---|
| Application shape | HTTP-triggered functions calling Atlas application use cases |
| Host operations | Managed function runtime; Atlas owns handlers, dependencies, and configuration |
| Runtime | Functions 4.x supports .NET 10 in the isolated worker model; use Flex Consumption for Linux .NET 10 [S6] |
| Billing | Active instance memory/time and executions; always-ready instances add baseline charges [S7, S8] |
| Atlas work | Add an HTTP host adapter and preserve request-scoped actor, authorization, and DbContext lifetimes |
| Main experiment | Measure billing and latency with realistic concurrency, SQL access, and startup |

Functions do not require moving domain logic into individual handlers or deploying every domain separately. Reuse Atlas's application boundaries. The HTTP authentication and authorization integration still needs implementation; choosing a function host does not supply those Atlas policies automatically.

Flex Consumption and legacy Consumption have different meters and free grants. Flex currently lists 250,000 executions and 100,000 GB-seconds of on-demand grant per subscription/month. Do not apply the legacy plan's larger grant to a Flex estimate. Always-ready usage has separate charges. [S7, S8]

### Candidate B: Azure Container Apps Consumption + Azure SQL Database

| Attribute | Research finding |
|---|---|
| Application shape | A conventional ASP.NET Core API packaged as a Linux container |
| Host operations | Managed compute hosts; Atlas maintains its container image |
| Billing | Allocated vCPU-seconds, GiB-seconds, and requests [S9] |
| Scale-to-zero | Supported with suitable scaling rules and minimum replicas of zero [S10] |
| Warm capacity | Minimum replicas above zero can incur idle or active charges [S11] |
| Main experiment | Compare zero versus one minimum replica using the same API and database |

This is the initial research preference for a conventional REST API because it preserves familiar ASP.NET Core hosting while providing serverless compute. That is an architectural inference, not a cost result. Functions remains a primary comparison candidate, especially for event-driven work.

Container Apps currently lists monthly grants of 180,000 vCPU-seconds, 360,000 GiB-seconds, and two million requests per subscription. These are shared allowances, not an independent grant for every app or environment. [S9]

### Database option: General Purpose serverless

Azure SQL serverless scales database compute and can pause when inactive; storage remains billable during a pause. While online, billing accounts for CPU, memory, and configured minimums. It is not simply a charge for each query. [S12, S13]

Open SQL sessions can prevent auto-pause. On resume, the first connection can fail temporarily, so test connection retries and the user-visible delay. Frequent database health probes, background jobs, and connection-pool behavior must be included in the experiment. A backend scaling to zero does not prove that the database pauses. [S14]

Compare serverless against a small provisioned DTU or vCore tier that meets the same workload. DTUs bundle compute and I/O capacity; serverless is available through the vCore model. A provisioned database may cost less for persistent activity or deliver more consistent response times, even when the backend remains serverless. That outcome needs measurement. [S15]

## Provisioned and AWS comparison options

### Azure App Service + Azure SQL Database

Use a dedicated App Service plan as the Azure baseline for predictable application capacity. Atlas deploys its API without operating the underlying VM. The plan is charged for provisioned instances, and apps in the same plan share its capacity. Check the selected tier's networking, scaling, and deployment features rather than assuming every tier includes them. [S1, S16]

Compare Linux pricing first if the future API has no Windows-only dependency. The public pricing page did not expose a reliable selected-region dollar figure in this research, so the worksheet below leaves the quote pending. [S17]

### AWS Elastic Beanstalk + RDS for SQL Server

Beanstalk has no additional service charge; the bill comes from the resources used by the environment. Include EC2, database, storage, and any load balancer or other networking resources actually selected. It reduces deployment work but retains an EC2-based environment and shared platform-update responsibilities. [S2, S18, S19]

RDS for SQL Server is the managed database comparison. Quote a production-appropriate edition and licensing model; include storage, backup retention, and the selected availability configuration. License Included covers SQL Server licensing, while BYOM has eligibility requirements. Developer Edition is for non-production use. [S20]

### AWS Lightsail

Lightsail Containers can host a containerized API with bundled capacity pricing. Its Nano example is $7/month for 0.25 shared vCPU and 512 MB; that is an application-capacity reference, not a sizing recommendation or an Atlas total. Capacity grows by selecting larger power or additional nodes. [S3, S21]

Lightsail managed databases offer MySQL and PostgreSQL, which do not match Atlas's accepted SQL Server direction. An external SQL Server arrangement needs its own cost and connectivity evaluation. Do not assume Lightsail instance networking capabilities also apply to the container service. [S22]

### AWS Lambda and ECS with Fargate

Lambda can host ASP.NET applications through AWS integration; it does not necessarily require rewriting every endpoint as a separate domain function. Evaluate host adaptation, cold starts, database connections, and gateway costs. [S23]

Fargate runs containers without Atlas administering EC2 hosts, but billing follows allocated resources and runtime. An always-running task is not billed only while handling a request. Its service, ingress, and network configuration add decisions compared with a simple managed web platform. Keep it as a later comparison if container requirements justify that work. [S24, S25]

## What a useful cost comparison includes

Compare complete deployments satisfying the same performance and availability targets:

```text
Monthly cloud cost = application compute + database compute/storage
                   + API ingress/gateway + data transfer/CDN
                   + logs/metrics + backups + image/artifact storage
                   + secrets/network services + other environments
```

Record management time separately: patching responsibilities, deployment and rollback effort, incident response, database administration, and provider-specific learning. A smaller cloud bill can require more maintenance time.

Show ordinary paid usage, eligible recurring grants, and temporary credits as separate totals. Do not choose a platform because the introductory invoice happens to be zero. Price a lean pilot and a resilient production configuration separately; a single application instance and a single-AZ database are not equivalent to a redundant deployment.

### Workload assumptions to test

These are comparison scenarios, not forecasts of Atlas adoption:

| Scenario | Monthly API requests |
|---|---|
| Quiet pilot | 100,000 |
| Regular use | 1,000,000 |
| Growth | 10,000,000 |

For each scenario, record burst concurrency, request mix, response size, database growth, warm/cold latency, and hours of activity. A million requests spread across a month can have different capacity needs from a million arriving in a short burst. One page view may also issue several API requests.

### Azure examples: usage units before dollar quotes

For Container Apps, a hypothetical 0.25-vCPU/0.5-GiB replica running for 100 hours consumes **90,000 vCPU-seconds and 180,000 GiB-seconds**. At 730 hours it consumes **657,000 vCPU-seconds and 1,314,000 GiB-seconds**. These are our calculations from allocated resources, not measured Atlas usage. Separate active and idle meters and subtract only eligible unused grants. Concurrent requests can share a replica; do not multiply every request's duration by full replica capacity. [S9, S11]

For Functions Flex, use active instance periods, provisioned memory, executions, and the plan's minimum billing/rounding rules. Summing individual HTTP durations is insufficient when requests run concurrently in one instance. Export observed billing metrics instead. [S26]

For SQL serverless, assume a hypothetical workload billed at one vCore during three online hours each day over 30 days: **90 billed vCore-hours**. If activity or open sessions keep it online all day, that becomes **720 billed vCore-hours**, before any extra resource use. At unit price `q` per vCore-hour, compute costs are `90 × q` and `720 × q`; add storage and other charges. The three-hour assumption includes the pause delay. Actual CPU/memory billing can increase these units. This illustrates why real pause behavior matters. [S13, S14]

### AWS numerical cross-check: cost depends on the work per request

Use AWS's published illustrative US East (N. Virginia) x86 Lambda rate of $0.0000166667/GB-second, $0.20/million invocations, and the API Gateway HTTP API first-tier example of $1/million calls. Assume one invocation and one small, billable HTTP API call per request. Exclude free grants, discounts, and all other services. [S27, S28]

```text
Compute + invocation + gateway subtotal
  = requests × (memory_GB × billed_seconds × 0.0000166667
                + 0.20 / 1,000,000 + 1.00 / 1,000,000)
```

| Monthly requests | 0.5 GB, 0.2 seconds | 1 GB, 1 second |
|---|---|---|
| 100,000 | $0.29 | $1.79 |
| 1,000,000 | $2.87 | $17.87 |
| 10,000,000 | $28.67 | $178.67 |

These are calculated subtotals, not Atlas quotes or Azure prices. Database, logs, networking, storage, startup overhead, and extra invocations can change the result. HTTP API is an AWS product name; a RESTful Atlas API does not automatically need AWS's separately priced REST API product.

With a hypothetical equivalent provisioned application subtotal of $40/month, this simplified crossover occurs near 14 million requests for the lighter case and 2.24 million for the heavier case. Neither number proves that a $40 configuration can serve that workload. Compare measured capacity and full costs before drawing a hosting conclusion. Serverless cost is not inherently exponential as traffic grows; the work performed and service meters determine it.

## Repeatable research and decision process

1. **Set constraints.** Record a monthly budget, acceptable cold-start latency, warm-response target, availability, data location, and recovery point/time targets. These are pending decisions, not new committed requirements.
2. **Build comparable estimates.** Use one suitable Azure region for the Azure candidates and one suitable AWS region for AWS. Co-locate backend and database. Record currency, date, exact SKU, OS, resource sizes, availability, network topology, and every priced resource.
3. **Use official calculators.** Save an [Azure calculator](https://azure.microsoft.com/en-us/pricing/calculator/) or [AWS calculator](https://calculator.aws/) export or share link plus inputs in version control. If a dynamic page omits a price, mark it pending rather than inventing it.
4. **Run a representative slice.** Once a new HTTP host exists, measure browsing/discovery, an authenticated mutation, and Identity registration/login. Use realistic SQL data, not an endpoint returning a constant.
5. **Exercise startup and idle behavior.** Measure API cold starts, SQL resume, retries, connection pooling, probes, and event/background activity. Compare zero and warm minimum capacity. Observe a full idle interval with database clients disconnected.
6. **Verify deployment and recovery.** Run migrations and SQL tests against the candidate, verify managed identity and database permissions, test persistent Data Protection keys where required, and demonstrate a restore. Application rollback alone does not undo a schema migration.
7. **Compare evidence.** Record the complete bill estimate and management effort at each workload, then accept or revise ADR-0007. Repeat when workload, pricing, or operational needs materially change.

Do not provision resources as part of this documentation PR. A deployment experiment needs separately agreed budget and configuration.

### Estimate worksheet

Keep one record per exact backend/database combination. Copy this template when regional quotes are available:

| Attribute | Value to record |
|---|---|
| Candidate | Functions Flex / Container Apps Consumption / App Service / AWS comparison |
| Quote provenance | Calculator export or link, retrieval date, region, currency |
| Backend | SKU, memory/CPU, minimum/maximum instances, concurrency, activity duration |
| Database | Product, tier, resources, storage, availability, pause delay, backup retention |
| Networking | Ingress, database access path, private endpoints/NAT if used, egress |
| Supporting services | Logs, image registry, Functions storage, secrets, frontend hosting |
| Pilot / regular / growth | Complete monthly estimate for each workload |
| Grants and credits | Eligibility, shared usage, expiry, and undiscounted total |
| Evidence | Test results, warm/cold latency, billed usage, operations effort |
| Current status | Pending regional quote and representative deployment measurements |

### Cost controls and revision triggers

Use explicit replica/instance limits and database capacity limits, retention settings, and budget alerts. Azure budget alerts notify; they do not automatically stop resources or create a hard spending cap. Instance limits also do not cap storage, egress, or every other service charge. [S29]

Revisit the decision when measured spending exceeds the agreed budget, cold/resume latency fails the agreed target, SQL connections or query capacity become a bottleneck, or recovery/availability requires a different configuration. Measure before committing to reserved capacity or splitting Atlas into independently deployed services.

## Sources

All sources below are official provider documentation or pricing pages, consulted on 2026-10-09. Published rates and supported features may change. Dollar examples identify their scope; selected-region Azure/AWS totals remain pending.

- **S1:** [Azure App Service overview](https://learn.microsoft.com/en-us/azure/app-service/app-service-web-overview)
- **S2:** [AWS decision guide: Lightsail, Beanstalk, and EC2](https://docs.aws.amazon.com/decision-guides/latest/decision-guides/lightsail-elastic-beanstalk-ec2.html)
- **S3:** [Lightsail container service concepts and capacity](https://docs.aws.amazon.com/lightsail/latest/userguide/amazon-lightsail-container-services.html)
- **S4:** [Azure SQL Database overview](https://learn.microsoft.com/en-us/azure/azure-sql/database/sql-database-paas-overview?view=azuresql)
- **S5:** [Azure SQL engine feature comparison](https://learn.microsoft.com/en-us/azure/azure-sql/database/features-comparison?view=azuresql)
- **S6:** [Azure Functions .NET isolated worker guide](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide)
- **S7:** [Functions Flex Consumption hosting and billing](https://learn.microsoft.com/en-us/azure/azure-functions/flex-consumption-plan)
- **S8:** [Azure Functions pricing](https://azure.microsoft.com/en-us/pricing/details/functions/)
- **S9:** [Azure Container Apps pricing](https://azure.microsoft.com/en-us/pricing/details/container-apps/)
- **S10:** [Container Apps scaling rules](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)
- **S11:** [Container Apps billing](https://learn.microsoft.com/en-us/azure/container-apps/billing)
- **S12:** [Azure SQL serverless overview](https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-overview?view=azuresql)
- **S13:** [Azure SQL serverless billing](https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-billing?view=azuresql-db)
- **S14:** [Azure SQL auto-pause and auto-resume](https://learn.microsoft.com/en-us/azure/azure-sql/database/serverless-tier-auto-pause-resume?view=azuresql-db)
- **S15:** [Azure SQL DTU purchasing model](https://learn.microsoft.com/en-us/azure/azure-sql/database/service-tiers-dtu?view=azuresql)
- **S16:** [App Service plans and billing](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans)
- **S17:** [App Service Linux pricing](https://azure.microsoft.com/en-us/pricing/details/app-service/linux/)
- **S18:** [Elastic Beanstalk pricing](https://aws.amazon.com/elasticbeanstalk/pricing/)
- **S19:** [Beanstalk platform shared responsibility](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/platforms-shared-responsibility.html)
- **S20:** [RDS for SQL Server pricing and licensing](https://aws.amazon.com/rds/sqlserver/pricing/)
- **S21:** [Lightsail pricing](https://aws.amazon.com/lightsail/pricing/)
- **S22:** [Lightsail managed databases](https://docs.aws.amazon.com/lightsail/latest/userguide/amazon-lightsail-databases.html)
- **S23:** [ASP.NET applications on Lambda](https://docs.aws.amazon.com/lambda/latest/dg/csharp-package-asp.html)
- **S24:** [Fargate for ECS](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- **S25:** [Fargate pricing](https://aws.amazon.com/fargate/pricing/)
- **S26:** [Functions consumption cost estimation](https://learn.microsoft.com/en-us/azure/azure-functions/functions-consumption-costs)
- **S27:** [Lambda pricing](https://aws.amazon.com/lambda/pricing/)
- **S28:** [API Gateway pricing](https://aws.amazon.com/api-gateway/pricing/)
- **S29:** [Azure budgets and alerts](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets)
