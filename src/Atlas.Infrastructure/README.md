# Atlas.Infrastructure

Reusable host-independent infrastructure implementing domain repository contracts above the existing Atlas.Persistence EF model and Atlas.Identity account infrastructure.

- Storage contains SQL repository adapters, shared transaction/reference coordination and opt-in demo seed support.
- SqlStorage configures the current SQL backend and applies existing migrations when the host explicitly invokes Configure. Inject context factories into repositories for isolated tests or request-specific composition.
- LocalDevelopmentTelemetry owns development-only local files and audit delivery. It accepts an optional host error-reporting callback; it does not write to a terminal itself. Keep future production/cloud wiring in a separate adapter/file.

This project does not reference the Console executable. Neither moving adapters nor introducing this project changes the EF schema, migrations, shared transaction or outbox lease semantics. See [architecture](../../docs/architecture/APPLICATION-WORKFLOWS.md).
