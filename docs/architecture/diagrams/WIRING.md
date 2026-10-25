# Application wiring and presentation ownership

[Visual guide](README.md) · [Ownership guide](../APPLICATION-WORKFLOWS.md) · [Transaction details](TRANSACTIONS-AND-EVENTS.md)

Atlas has two backend hosts: Console and the JSON REST API. Each host translates its own interaction into reusable application/domain operations and chooses infrastructure at startup. **Thin describes responsibility, not program length.** Console owns its terminal presentation; the API owns HTTP behavior and JSON contracts. Browser presentation belongs to SvelteKit. Storybook imports the same frontend components to exercise them independently of application routes.

## Presentation reaches a shared backend

```mermaid
flowchart TD
    subgraph Web["Browser presentation: Atlas.Web"]
        Routes["SvelteKit routes"] --> UI["Shared Svelte components and API clients"]
        Stories["Storybook stories"] --> UI
    end
    subgraph Console["Atlas.Console host"]
        CRoot["Program: startup composition"] --> Commands["Menus, commands and terminal rendering"]
    end
    subgraph Api["Atlas.Api host"]
        ARoot["Program and ApiServices: startup composition"] --> HTTP["Middleware, controllers and JSON mapping"]
    end
    UI -->|HTTP requests| HTTP
    Commands --> Backend["Application workflows and domain services"]
    HTTP --> Backend
```

The arrows show composition or request flow, as labeled; they are not a complete assembly-reference graph. Console and the API are independent entry points. Neither calls the other. Both can use the same configured database and reuse the same backend operations. SvelteKit does not load .NET domain entities or connect directly to SQL.

Storybook owns stories, preview configuration and navigation adapters. Production components, styles and API clients live under `Atlas.Web/src`; they are reused by routes and stories. Isolated examples and browser tests can simulate responses, while live compositions use the real API. Passing an isolated story does not establish live backend correctness.

## Startup chooses implementations and lifetimes

```mermaid
flowchart TD
    Roots["Console Program or API ApiServices"] --> Workflows["Application workflows"]
    Roots --> Adapters["Application.Storage SQL adapters"]
    Roots --> Identity["Atlas.Identity infrastructure"]
    Workflows --> Domains["Domain services and rules"]
    Workflows --> Ports["Domain-owned repository ports"]
    Domains --> Ports
    Adapters -. implements .-> Ports
    Adapters --> EF["Atlas.Persistence: EF model and context"]
    Identity --> EF
    EF --> Database["SQL Server"]
```

The grouped boxes are roles, not classes. Ports include `INodeRepository`, `IDocumentRepository` and `IVoteRepository`. `Atlas.Application` contains cross-domain workflows **and concrete SQL/observability adapters**; it therefore references Persistence and Identity. It is host-independent, not a persistence-independent assembly. Domain rules consume ports rather than EF types. Some single-domain operations can be called directly by a host without a pass-through application wrapper.

| Responsibility | Current source |
|---|---|
| Terminal composition, presentation and console session | [Console Program](../../../src/hosts/Atlas.Console/Program.cs), [ConsoleApplication](../../../src/hosts/Atlas.Console/ConsoleApplication.cs) |
| HTTP pipeline and dependency injection | [API Program](../../../src/hosts/Atlas.Api/Program.cs), [ApiServices](../../../src/hosts/Atlas.Api/ApiServices.cs) |
| HTTP identity, operation context and error mapping | [ApiBoundary](../../../src/hosts/Atlas.Api/ApiBoundary.cs) |
| Browser routes and reusable UI | [SvelteKit routes](../../../src/hosts/Atlas.Web/src/routes), [components](../../../src/hosts/Atlas.Web/src/lib/components), [API clients](../../../src/hosts/Atlas.Web/src/lib/api) |
| Storybook imports and preview setup | [Stories](../../../src/hosts/Atlas.Web/storybook), [main.ts](../../../src/hosts/Atlas.Web/.storybook/main.ts), [preview.ts](../../../src/hosts/Atlas.Web/.storybook/preview.ts) |
| Use cases and concrete repository implementations | [Application](../../../src/Atlas.Application/README.md), [Storage](../../../src/Atlas.Application/Storage) |
| EF mappings and persistence/audit integration | [AtlasDataContext](../../../src/Atlas.Persistence/AtlasDataContext.cs) |
| Host and domain dependency checks | [ArchitectureTests](../../../tests/Atlas.Application.Tests/ArchitectureTests.cs) |

Console constructs most collaborators manually and uses a service collection for Identity. The API uses dependency injection: repository adapters, workflows, the event publisher and `RequestActor` are request-scoped; the context factory, discovery cache and telemetry have host lifetime. The factory creates fresh contexts rather than sharing one context across requests. Console owns/disposes its discovery cache; API DI owns its singleton cache. The two processes do not share cache memory. See [discovery wiring and cache lifetime](../discovery-read-query.md#host-wiring-and-cache-lifetime).

Hosts establish verified identity. Console retains its terminal sign-in/session behavior; API middleware resolves an eligible participant from the authenticated principal and supplies `RequestActor` to controller operations. Client JSON or a supplied participant GUID is not authentication. Domain/application ownership checks remain authoritative even when the frontend hides edit controls.

## Local development and diagnostics

The [shared Vite proxy](../../../src/hosts/Atlas.Web/dev/localApi.ts) is used by the app's [Vite configuration](../../../src/hosts/Atlas.Web/vite.config.ts) and Storybook's development configuration. Public routes forward anonymous allowlisted reads; session routes allow selected authenticated reads/writes and preserve backend cookie/CSRF protections. This is development routing, not the production hosting/security boundary. Static builds need equivalent deployment routing. The API remains JSON-only; it does not render the SvelteKit interface.

`Atlas.RankingLab` is an additional local diagnostics host, not a production API or another application presentation layer. The ranking story uses a separate loopback bridge to read the local Atlas database and explain the same persistence search query. It does not create/reset application data. Controlled SQL fixtures remain regression-test inputs separately from the live workbench. See [ranking diagnostics](../../discovery/search-ranking-workbench.md) and [frontend startup](../../../src/hosts/Atlas.Web/README.md).

## Events are wired separately from repository dependencies

```mermaid
classDiagram
    direction TB
    class IEventPublisher {
        <<interface>>
        Publish(message)
    }
    class InMemoryEventPublisher {
        Subscribe(handler)
        Publish(message)
    }
    class NodeWorkflowService
    class CommentWorkflowService
    class ModerationWorkflowService
    class ObserveNodeLifecycleInContent {
        Handle(NodeCreatedV1)
        Handle(NodeArchivedV1)
    }
    class NotificationService {
        Handle(NotificationRequestedV1)
    }
    IEventPublisher <|.. InMemoryEventPublisher
    NodeWorkflowService ..> IEventPublisher
    CommentWorkflowService ..> IEventPublisher
    ModerationWorkflowService ..> IEventPublisher
    InMemoryEventPublisher ..> ObserveNodeLifecycleInContent : registered handler
    InMemoryEventPublisher ..> NotificationService : registered handler
```

Console Program and API `ApiServices` register the Content observer for `NodeCreatedV1` and `NodeArchivedV1`, and NotificationService for `NotificationRequestedV1`. The Content observer confirms document existence and optionally reports a diagnostic observation; it does not create/delete the description. The description is already saved during creation and retained when archived.

See [publisher](../../../src/Atlas.Application/Eventing/InMemoryEventPublisher.cs), [Content observer](../../../src/Atlas.Application/Content/ObserveNodeLifecycleInContent.cs) and [Console subscriptions](../../../src/hosts/Atlas.Console/Program.cs) and [API registrations](../../../src/hosts/Atlas.Api/ApiServices.cs). [Event failure semantics](TRANSACTIONS-AND-EVENTS.md#event-delivery-after-saving) matter when reviewing these arrows.

## Extending or replacing a host

Reuse application workflows, domain policies, contracts and appropriate adapters. A new host supplies interaction/presentation mapping, verified identity, configuration, service lifetimes, subscriptions and error handling. It must reload server-side state rather than trust client-supplied domain objects. Replacing SQL also requires adapters that satisfy the existing transaction, reference-validation and topology guarantees; implementing repository signatures alone is insufficient.

The current examples are the implemented Console and API hosts, not a proposal to add the API later. [Detailed workflow ownership](../APPLICATION-WORKFLOWS.md) and the [API code walkthrough](../../api/CODE-GUIDE.md) provide the next level of detail.
