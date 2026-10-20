# Atlas.Application

The reusable backend shared by application hosts. It contains cross-domain workflows, integration adapters, recipient selection, synchronous event dispatch and bounded queries, alongside concrete SQL storage and local-development observability adapters.

- `Storage/`: repository implementations, transaction/reference coordination, SQL startup and opt-in demo seeds.
- `Observability/`: local file output and recoverable audit delivery lifecycle. Future production adapters belong in separate files/classes.
- `GlobalUsings.cs`: common operation contracts for this project.
- Workflow classes and domain integration folders: reusable application behavior.

Application references domain projects, Contracts, Persistence and Identity. It has no dependency on Console or terminal APIs. Domain projects retain their own repository interfaces and business rules. The repository-level `infrastructure/` folder is reserved for infrastructure as code, not runtime adapters.

Console lives under `src/hosts/Atlas.Console`; the JSON API lives under `src/hosts/Atlas.Api`. Both consume this backend. See [HTTP workflow mapping](../../docs/api/WORKFLOWS.md).

See [ownership and composition](../../docs/architecture/APPLICATION-WORKFLOWS.md) and the [visual architecture guide](../../docs/architecture/diagrams/README.md).
