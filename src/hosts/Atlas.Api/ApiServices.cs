using Atlas.Application;
using Atlas.Application.Comments;
using Atlas.Application.Content;
using Atlas.Application.Eventing;
using Atlas.Application.Storage;
using Atlas.Application.Voting;
using Atlas.Comments.Comments;
using Atlas.Communities.Communities;
using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;
using Atlas.Content.Documents;
using Atlas.Contracts.Events;
using Atlas.Contracts.Graph.V1;
using Atlas.Contracts.Notifications.V1;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Identity;
using Atlas.Moderation;
using Atlas.Notifications;
using Atlas.Participants.Participants;
using Atlas.Persistence;
using Atlas.Persistence.Discovery;
using Atlas.Voting;
using Atlas.Voting.Data;
using Atlas.Voting.Eligibility;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Api;

/// <summary>API composition root. Repositories use the configured database without migrating it at startup.</summary>
public static class ApiServices
{
    /// <summary>Choose the shared backend implementations and their lifetimes for the HTTP host.</summary>
    /// <remarks>Registration describes how to create services; scoped services are resolved later per request.</remarks>
    public static IServiceCollection AddAtlasApi(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Identity and repository factories target the same database as Console.
        var connection = configuration["ATLAS_SQL_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Set ATLAS_SQL_CONNECTION_STRING.");
        services.AddAtlasIdentity(connection);
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        Func<AtlasDataContext> open = () => new AtlasDataContext(options);
        // Singleton here is the reusable factory delegate, not a shared DbContext.
        // Every call creates a fresh context; SQL operation boundaries coordinate transactions.
        services.AddSingleton(open);

        // 2. Each request gets its own repository adapters. Interfaces belong to domains;
        // concrete SQL implementations stay in the shared Application project.
        services.AddScoped<INodeRepository>(_ => new SqlNodeRepository(open));
        services.AddScoped<INodeTypeRepository>(_ => new SqlNodeTypeRepository(open));
        services.AddScoped<IDocumentRepository>(_ => new SqlDocumentRepository(open));
        services.AddScoped<IParticipantRepository>(_ => new SqlParticipantRepository(open));
        services.AddScoped<IVoteRepository>(_ => new SqlVoteRepository(open));
        services.AddScoped<IReactionDefinitionRepository>(_ => new SqlReactionDefinitionRepository(open));
        services.AddScoped<INodeReactionRepository>(_ => new SqlNodeReactionRepository(open));
        services.AddScoped<ICommunityRepository>(_ => new SqlCommunityRepository(open));
        services.AddScoped<ICommunityMembershipRepository>(_ => new SqlCommunityMembershipRepository(open));
        services.AddScoped<ICommunityNodeRepository>(_ => new SqlCommunityNodeRepository(open));
        services.AddScoped<ICommentRepository>(_ => new SqlCommentRepository(open));
        services.AddScoped<IModerationCaseRepository>(_ => new SqlModerationCaseRepository(open));
        services.AddScoped<INotificationRepository>(_ => new SqlNotificationRepository(open));

        // 3. Wire in-app notifications and synchronous event subscribers once per scope.
        // The empty delivery list means no external email/push sender is simulated here.
        services.AddScoped<NotificationService>(s => new(s.GetRequiredService<INotificationRepository>(), []));
        services.AddScoped<IEventPublisher>(s =>
        {
            // This dispatcher is local and non-durable; security audit uses a separate SQL outbox.
            var publisher = new InMemoryEventPublisher();
            var content = new ObserveNodeLifecycleInContent(s.GetRequiredService<IDocumentRepository>());
            publisher.Subscribe<NodeCreatedV1>(content.Handle);
            publisher.Subscribe<NodeArchivedV1>(content.Handle);
            publisher.Subscribe<NotificationRequestedV1>(x => s.GetRequiredService<NotificationService>().Handle(x));
            return publisher;
        });

        // 4. Reuse application/domain use cases; controllers only translate HTTP input/output.
        services.AddScoped<IVotingEligibility, RepositoryVotingEligibility>();
        services.AddScoped<VoteMutationPolicy>();
        services.AddScoped<CastVote>();
        services.AddScoped<UndoVote>();
        services.AddScoped<NodeWorkflowService>();
        services.AddScoped<NodeDescriptionService>();
        services.AddScoped<ReactionVotingService>();
        services.AddScoped<CommentWorkflowService>();
        services.AddScoped<CommunityService>();
        services.AddScoped<ModerationService>();
        services.AddScoped<ModerationWorkflowService>();

        // 5. Share the read cache across requests, but keep queries and actors request-scoped.
        services.AddSingleton<DiscoveryCache>();
        services.AddScoped<IDiscoveryService>(s => new DiscoveryService(new SqlDiscoveryReadQuery(open, s.GetRequiredService<DiscoveryCache>())));
        services.AddScoped<RequestActor>();
        services.AddScoped<DevelopmentAccountMail>();
        // Local telemetry has host lifetime. The hosted worker owns its cancellation/shutdown.
        services.AddSingleton<ApiTelemetry>();
        services.AddHostedService<ApiAuditWorker>();

        // 6. Adapt the existing Identity cookie to a JSON API. IdentityServices retains
        // secure/HttpOnly and principal validation; these options add API-specific behavior.
        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = "__Host-Atlas.Session"; o.Cookie.SameSite = SameSiteMode.Lax;
            o.ExpireTimeSpan = TimeSpan.FromHours(8); o.SlidingExpiration = false;
            o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = 401; return Task.CompletedTask; };
            o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = 403; return Task.CompletedTask; };
        });
        return services;
    }
}
