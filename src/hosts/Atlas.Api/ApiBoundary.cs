using Atlas.Contracts.Identity;
using Atlas.Contracts.Observability;
using Atlas.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api;

/// <summary>Set only after resolving an active, confirmed Identity account for this request.</summary>
public sealed class RequestActor : IAuthenticatedActor
{
    internal Guid? Id { get; set; }
    public Guid ParticipantId => Id ?? throw new UnauthorizedAccessException();
}

public sealed class OperationContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AtlasAccounts accounts, RequestActor actor, ApiTelemetry telemetry)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var participant = await accounts.ResolveParticipantAsync(context.User, context.RequestAborted);
            if (participant is null) { context.Response.StatusCode = 401; return; }
            actor.Id = participant.Value;
        }
        using var observer = telemetry.Observer is null ? null : Telemetry.UseObserver(telemetry.Observer);
        using var operation = Telemetry.BeginOperation(actor.Id);
        await next(context);
    }
}

/// <summary>Translate known rejection categories without returning SQL, stack traces or submitted content.</summary>
public sealed class ApiErrors(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken cancellationToken)
    {
        if (error is OperationCanceledException && context.RequestAborted.IsCancellationRequested) return false;
        var retry = error switch
        {
            Atlas.Graph.Nodes.NodeCreationQuotaExceededException e => e.Allowance.RetryAfter,
            Atlas.Voting.VoteActionQuotaExceededException e => e.Allowance.RetryAfter,
            Atlas.Communities.Communities.CommunityCreationQuotaExceededException e => e.Allowance.RetryAfter,
            Atlas.Moderation.ReportSubmissionQuotaExceededException e => e.Allowance.RetryAfter,
            _ => (TimeSpan?)null
        };
        var quota = retry is not null;
        if (retry is {} delay) context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(delay.TotalSeconds)).ToString(System.Globalization.CultureInfo.InvariantCulture);
        var status = error switch
        {
            UnauthorizedAccessException => 403,
            KeyNotFoundException => 404,
            ArgumentException => 400,
            InvalidOperationException when quota => 429,
            InvalidOperationException => 409,
            _ => 500
        };
        context.Response.StatusCode = status;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails { Status = status, Extensions = { ["code"] = quota ? "quota_exceeded" : "request_failed" }, Title = status switch
            {
                400 => "Invalid request.", 403 => "This operation is not permitted.", 404 => "Resource not found.",
                429 => error.Message,
                409 => "The operation conflicts with the current state.", _ => "An unexpected error occurred."
            }}
        });
    }
}

/// <summary>Enforce CSRF on every write, including anonymous login and registration.</summary>
public sealed class CsrfFilter : Microsoft.AspNetCore.Mvc.Filters.IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method)) return;
        try { await context.HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
            .ValidateRequestAsync(context.HttpContext); }
        catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
        { context.Result = new BadRequestObjectResult(new ProblemDetails { Status = 400, Title = "A valid CSRF token is required." }); }
    }
}
