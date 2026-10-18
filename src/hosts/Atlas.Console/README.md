# Atlas.Console: terminal host

Console lives under `src/hosts/Atlas.Console`. It owns keyboard input, menus, navigation, terminal rendering, the interactive Identity session and startup composition.

Program selects the SQL implementations in `Atlas.Application.Storage`, configures Identity, creates `Atlas.Application.Observability.LocalDevelopmentTelemetry`, registers event handlers and supplies console diagnostic callbacks. Commands call application/domain services and display their results. View-specific repository reads remain appropriate here; mutation rules and save ordering belong in the backend.

`Atlas.Application` contains reusable workflows, storage and observability. `Atlas.Persistence` retains the EF model/migrations, and `Atlas.Identity` retains account infrastructure. The `infrastructure/` directory is reserved for infrastructure as code.

- [Thin-host ownership and API composition](../../../docs/architecture/APPLICATION-WORKFLOWS.md)
- [UML and process diagrams](../../../docs/architecture/diagrams/README.md)
- [Identity setup and account entry](../../../docs/PTT-94-Identity.md#use-registration-and-sign-in)

The current application runs in one process. Domain events publish synchronously after the relevant save boundary returns; subscriber failure does not undo committed business data. The audit outbox is a separate durable pipeline.
