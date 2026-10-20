# Controller attributes, execution order and responses

Start with [CommunitiesController](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs),
then [Program](../../src/hosts/Atlas.Api/Program.cs) and [ApiBoundary](../../src/hosts/Atlas.Api/ApiBoundary.cs).
This guide explains the framework mechanisms used throughout the controllers.

## Attributes describe an endpoint; they are not a top-to-bottom script

C# attributes are metadata attached to a class, method or parameter. ASP.NET Core
reads that metadata when discovering endpoints and executing requests. The `Attribute`
suffix is normally omitted: `[Authorize]` means `AuthorizeAttribute`.

| Attribute or syntax | Atlas use | Meaning |
|---|---|---|
| `[ApiController]` | Controller class | API conventions: attribute routing, inferred parameter binding sources and automatic 400 responses for invalid model state. It does not authenticate callers. |
| `[Route("api/communities")]` | Controller class | Common route prefix, combined with relative action templates. |
| `[HttpGet("{id:guid}")]` | Read action | Match GET with the suffix and a GUID route value. The constraint selects a route; it is not a database existence/permission check. |
| `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` | Write actions | Select the HTTP verb and optional suffix. A matching verb does not itself authorize a write. |
| `[Authorize]` | Protected action/class | Require an authenticated identity. |
| `[Authorize(Policy = AtlasPolicies.Member)]` | Participant actions | Require Atlas's persisted role/eligibility policy; domain ownership rules still run in the workflow. |
| `[Authorize(Policy = AtlasPolicies.GlobalModerator)]` | Moderator decisions | Require the global moderator policy, in addition to any controller-level requirements. |
| `[AutoValidateAntiforgeryToken]` | Every controller; also registered globally | Validate unsafe methods such as POST/PUT/DELETE against the cookie/header CSRF pair. Normal GET/HEAD/OPTIONS reads are exempt. |
| `[EnableRateLimiting("auth")]` | AccountsController | Select the named account budget; the global concurrency limiter still applies. Middleware must run for the metadata to take effect. |
| `[FromQuery]` | Discovery query argument | Bind fields from URL query parameters rather than treating the complex object as a JSON body. |
| `[Required]`, `[EmailAddress]`, `[MaxLength(...)]` | Request record properties/parameters | Declarative input validation. They supplement domain rules and the server byte ceiling. |

`[ApiController, Route("...")]` on one line and two separate attribute lines convey
the same metadata. Moving `[Authorize]` above or below `[HttpPost]` does not change
which security check runs first. We group attributes consistently for readability.
Multiple authorization requirements combine; a weaker method-level attribute does
not erase a controller-level requirement. Anonymous account routes are deliberate
entry points, but still require CSRF and rate limits.

## What order actually matters?

There are three different ordering mechanisms:

1. **Middleware registration in Program.cs.** Requests pass through `Use...` calls
   in order; responses unwind in reverse. A stage can stop before later stages run.
   Routing must select an endpoint before endpoint-specific limiting; authentication
   establishes the principal before authorization evaluates it.
2. **MVC filter stages.** Authorization filters precede resource/action processing;
   binding/validation and action execution occur later; result filters surround result
   execution. A numeric order does not move a result filter ahead of authentication.
3. **Order and scope within the applicable filter stage.** Lower `IOrderedFilter.Order`
   values run earlier for before-processing and later while unwinding. With equal order,
   global/controller/action scopes supply nesting. Do not rely on attribute source order
   to resolve same-order filters.

Atlas's `AntiforgeryProblemDetailsFilter.Order = int.MinValue` makes its result handling
run before MVC's generic API client-error result conversion consumes the antiforgery
failure marker. It formats a failure that the framework has already detected; it does
not validate tokens itself. The global antiforgery registration protects future MVC
controllers, while explicit attributes make the policy visible on current controllers.
These MVC filters do not automatically protect separately mapped minimal API routes.

For a typical protected write:

- The outer HTTP telemetry scope starts, then exception handling wraps the request.
- HTTPS/header middleware runs, routing selects the action, and rate limiting admits it.
- Identity authenticates the cookie; authorization evaluates the endpoint policy.
- The participant adapter rechecks the persisted profile and enriches the operation actor.
- MVC validates CSRF, binds/validates the DTO and invokes the controller.
- The application/domain checks ownership/state/quotas; the controller returns a result.

A 401 can therefore occur before a missing-CSRF 400 on a protected route. Early
rate rejection can return 429 before either. Test each control with its prerequisites
satisfied if you want to exercise that particular boundary.

## Where ControllerBase gets its helpers

`public sealed class CommunitiesController(...) : ControllerBase` is inheritance.
`ControllerBase` comes from `Microsoft.AspNetCore.Mvc`, provided by ASP.NET Core's
shared framework through the Web SDK. These helpers are framework methods, not Atlas
utilities and not methods from Identity.

The base class also exposes the active `HttpContext`, `Request`, `Response`, `User`,
`ModelState` and URL helpers. MVC supplies the request's controller context; constructor
parameters such as `CommunityService` instead come from dependency injection. `Controller`
adds view-related features on top of `ControllerBase`; this JSON host needs the latter.

| Controller code | Response | What it does not imply |
|---|---|---|
| `Ok(value)` | 200 with formatted body, normally JSON | No extra database save or authorization. |
| `Ok()` | 200 without an explicit value | Not a command to write a response manually. |
| `return new { ... }` from an object-returning action | Normally 200 with JSON | It returns a projection, not the raw domain object unless we explicitly return one. |
| `Created(location, value)` | 201 with Location header and body | Does not create the entity; the preceding workflow did that. |
| `Accepted()` | 202 without a supplied body | Does not start a background job. Atlas registration already committed and wrote its development payload before returning it. |
| `NoContent()` | 204, no response body | Means **no content**, not “no change.” A successful edit often returns this. |
| `BadRequest(...)` | 400 | Not a substitute for domain validation. |
| `Unauthorized()` | 401 | Typically missing/invalid identity; distinct from forbidden permission. |
| `Forbid()` | Typically 403 via the authentication handler | Identity configuration determines challenge/forbid behavior; API redirects must remain disabled. |
| `NotFound()` | 404 | Can intentionally conceal hidden resources as well as represent missing data. |
| `Problem(statusCode: ..., title: ...)` | ProblemDetails response with the specified status | Must not receive raw exception, SQL, token or user-secret text. |

A result object describes the response; MVC executes and serializes it after the action
returns. `IActionResult` allows different results on success/failure. `Task<IActionResult>`
adds asynchronous work; `await` does not itself schedule a background job or send a reply.

`ApiErrors` handles thrown failures centrally, whereas a controller's returned
`Problem(...)` is already a response and does not need an exception. Framework-generated
validation, authorization and rate-limit failures also have their own paths. That is why
our tests check actual HTTP outcomes rather than assuming all errors come through one helper.

## Follow a small edit

For `CommunitiesController.Rename`:

1. The route and verb select the action; Member and CSRF checks protect the write.
2. MVC binds the route GUID and JSON `TextRequest` and applies its declared validation.
3. `Load(id)` resolves the current community; the workflow verifies ownership and rules.
4. The workflow persists the change; `NoContent()` describes its successful empty response.

The comments explain intent; method calls still supply the exact rules and transactions.

## Official references

- [Controller APIs and ApiController conventions](https://learn.microsoft.com/aspnet/core/web-api/?view=aspnetcore-10.0)
- [Filter stages, scope and Order](https://learn.microsoft.com/aspnet/core/mvc/controllers/filters?view=aspnetcore-10.0)
- [ControllerBase API](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-10.0)
- [CORS](https://learn.microsoft.com/aspnet/core/security/cors?view=aspnetcore-10.0)
- [Rate limiting](https://learn.microsoft.com/aspnet/core/performance/rate-limit?view=aspnetcore-10.0)
