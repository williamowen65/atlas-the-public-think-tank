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

/// <summary>Translate the validated HTTP principal into the actor and correlation scope used downstream.</summary>
/// <remarks>The primary constructor receives the next middleware delegate; ASP.NET supplies InvokeAsync services per request.</remarks>
public sealed class OperationContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AtlasAccounts accounts, RequestActor actor, ApiTelemetry telemetry)
    {
        // Recheck persisted account/profile eligibility rather than accepting an actor from JSON.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var participant = await accounts.ResolveParticipantAsync(context.User, context.RequestAborted);
            if (participant is null) { context.Response.StatusCode = 401; return; }
            actor.Id = participant.Value;
        }
        // IDisposable handles restore the previous AsyncLocal observer/context when the
        // downstream action finishes or throws. They are cleanup guards, not business data.
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
        // Only these known quota exceptions may contribute their user-facing message.
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
        // Other errors expose categories, never arbitrary exception/SQL text.
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

/// <summary>Keep built-in antiforgery rejections consistent with the API's JSON errors.</summary>
/// <remarks>This only formats a rejected result; ASP.NET Core performs token validation.</remarks>
public sealed class AntiforgeryProblemDetailsFilter : Microsoft.AspNetCore.Mvc.Filters.IAlwaysRunResultFilter, Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter
{
    // Run before ApiController's generic client-error mapping consumes the marker.
    public int Order => int.MinValue;

    public void OnResultExecuting(Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext context)
    {
        if (context.Result is Microsoft.AspNetCore.Mvc.Core.Infrastructure.IAntiforgeryValidationFailedResult)
            context.Result = new BadRequestObjectResult(new ProblemDetails
            { Status = 400, Title = "A valid CSRF token is required." });
    }

    public void OnResultExecuted(Microsoft.AspNetCore.Mvc.Filters.ResultExecutedContext context) { }
}
