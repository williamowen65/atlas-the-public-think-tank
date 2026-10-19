# Reading the API code

[Back to the API starting guide](README.md).

## Program: assemble the server

[Program.cs](../../src/hosts/Atlas.Api/Program.cs) uses **top-level statements**:
the compiler places the statements into the executable entry point. We do not
write a surrounding `Main` method ourselves.

Read it in three passes:

1. **Register services.** Describe how to obtain controllers, Identity, shared
   application workflows, CSRF checks, error handling and request limits.
2. **Build the middleware pipeline.** Each middleware receives a request and may
   reject it, modify it, or call the next delegate. Ordering matters: routing
   identifies the endpoint; authentication reads the cookie; authorization checks
   its policy; the operation scope surrounds the controller that follows.
3. **Map controllers and run.** The server begins listening. No view or frontend
   fallback is registered.

The empty `public partial class Program { }` joins the compiler-generated Program
type. `partial` permits the compiler-generated and explicit declarations to form
one type; `public` lets the separate test assembly name it. It provides the type
argument for `WebApplicationFactory<Program>`. It does not start another server
or create another Main method.

## Middleware: follow one request in order

See **region 03** of the [blackboard](../blackboards/workflows/Api/ApiWorkflows.excalidraw).
These are request stages, not startup service registrations:

| Order | Stage | What to look for |
|---|---|---|
| 1 | Exception handler | Wraps downstream exceptions; `ApiErrors` maps known failures safely |
| 2 | HSTS outside Development, HTTPS redirection | Establish transport expectations; use HTTPS locally for Secure cookies |
| 3 | Response headers | `no-store`, `nosniff`, `no-referrer` |
| 4 | Routing | Identify the selected endpoint before its policies and budgets run |
| 5 | Rate limiting | Host concurrency ceiling and account-route source budget; may stop with 429 |
| 6 | Authentication | Read/validate the cookie and establish `HttpContext.User` |
| 7 | Authorization | Enforce endpoint authentication/role policies; may stop with 401/403 |
| 8 | OperationContextMiddleware | Resolve the participant and open the telemetry operation scope |
| 9 | MVC filters and model binding | CSRF authorization filter on unsafe methods, then DTO binding/validation |
| 10 | Controller → Application → domains/persistence | Existing rules, quotas, transactions and event semantics; return JSON |

- Middleware can stop without calling `next`; later stages then do not run.
- Response execution unwinds through earlier middleware.
- Kestrel's 64 KiB body ceiling is a server setting, not a `Use...` stage.
- CSRF is an MVC filter within the controller endpoint, not a separate `Use...` middleware.
- Authentication establishes identity; CSRF establishes a valid request-token pair; domain authorization decides who may change the resource.

## ApiServices: choose implementations and lifetimes

[ApiServices.cs](../../src/hosts/Atlas.Api/ApiServices.cs) is the **composition root**:
the host connects domain interfaces to existing SQL adapters and application use cases.

| Registration | Lifetime and purpose |
|---|---|
| `AddScoped<T>()` | One instance per HTTP request scope; used for repositories, use cases and the verified actor |
| `AddSingleton<T>()` | One instance per application host; used for the read cache and local telemetry |
| `AddHostedService<T>()` | A background service whose startup/shutdown is managed by the host |

The singleton `Func<AtlasDataContext>` is a factory delegate. Calling it creates
a fresh context; it is not one singleton DbContext shared by concurrent requests.
Identity and these repositories target the same configured database as Console.

Event subscriptions are established when a request resolves its publisher. They
connect existing Content/Notifications handlers. This synchronous dispatcher is
separate from the durable SQL security-audit outbox.

## Controllers: HTTP adapters

A `[Route]` establishes a URL family and `[HttpGet]`, `[HttpPost]`, etc. choose the
operation. `[ApiController]` supplies API binding/validation conventions. Constructor
parameters are services obtained through dependency injection, not client JSON.

For example, [AccountsController](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs)
asks Identity to sign in/out, then returns an HTTP result. Its sign-out telemetry
records a bounded event, not a serialized account. Other controllers translate
request DTOs into existing use cases and project response data rather than exposing
EF rows. The authenticated actor comes from the request context.

Several classes use a **primary constructor**: parameters beside the class name
are constructor parameters available to the class’s implementation. This shortens
field/constructor boilerplate; it does not change dependency-injection lifetimes.

## CatalogController: reusable definitions

[CatalogController](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs)
manages shared catalogs that other workflows refer to:

- **Node types:** list/create definitions, edit permitted user definitions, archive them.
  - A new node chooses a `typeId` from these definitions.
  - Domain rules protect system definitions and enforce modification authority.
- **Reactions:** list/create reusable text/emoji definitions; moderators can suppress one.
  - Voting/applying a reaction to a particular node belongs to `VotesController`.
  - A definition ID and a node reaction **association ID** are different IDs.

See blackboard **region 07**, then the corresponding requests in `workflows.http`.

## ApiBoundary: the request scope

[ApiBoundary.cs](../../src/hosts/Atlas.Api/ApiBoundary.cs) resolves the active participant,
opens the ambient telemetry scope and maps rejection categories to HTTP errors.
It groups HTTP boundary responsibilities:

- `RequestActor`: the verified participant for this request.
- `OperationContextMiddleware`: account resolution and the ambient telemetry lifetime.
- `ApiErrors`: safe exception categories and quota retry information.
- `AntiforgeryProblemDetailsFilter`: consistent JSON formatting for CSRF rejection.

Its `using` variables are disposable cleanup guards:

- Leaving the scope restores the previous async context.
- Restoration also happens when downstream code throws.
- The old custom CSRF validation class is gone; the framework performs validation.

ASP.NET Core's `AutoValidateAntiforgeryToken` filter validates unsafe requests
before an action runs. It is registered globally as a default and declared on each
controller so reviewers and CodeQL can see the policy. `AddControllersWithViews()` registers the
framework services that this filter requires; it does not add view routes or
frontend files. `AntiforgeryProblemDetailsFilter` only formats rejected requests as
JSON; it does not perform or bypass validation. Cookies establish
the session; the CSRF header proves a write includes the paired antiforgery token.

## ApiFactory: exercise the real API inside a test

[ApiFactory](../../tests/Atlas.Api.Tests/BoundaryTests.cs) inherits
`WebApplicationFactory<Program>`. It starts the real startup code in an **in-process
test host**. It does not copy the API code, call an already-running server, or open
port 7240. Its HTTPS localhost address is a logical request address for TestServer.

The factory overrides environment/configuration and disables local telemetry output.
`Browser()` creates an HttpClient that keeps cookies. Redirects remain visible so
tests can assert 401/403 responses rather than accidentally following a login page.

Boundary tests need no database. The separate SQL journey supplies an isolated real
SQL Server database and exercises the same host with actual Identity and persistence.
That is why a test can validate cookie login without using your running application
or altering the database shared by your Console and API.

## What the solution filter does

[`tests/Atlas.Rewrite.Tests.slnf`](../../tests/Atlas.Rewrite.Tests.slnf) selects the
13 rewrite test projects from the root `Atlas.sln`, plus their build dependencies.

- It is not an API project, runtime service or duplicated source tree.
- It excludes the legacy application/test toolchain from focused rewrite runs.
- It now lives beside the test configuration under `tests/`.
- See [test strategy](../testing/TEST-STRATEGY.md) for commands and parallel settings.
