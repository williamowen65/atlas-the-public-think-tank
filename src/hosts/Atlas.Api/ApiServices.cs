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
    public static IServiceCollection AddAtlasApi(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration["ATLAS_SQL_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Set ATLAS_SQL_CONNECTION_STRING.");
        services.AddAtlasIdentity(connection);
        var options = new DbContextOptionsBuilder<AtlasDataContext>().UseSqlServer(connection).Options;
        Func<AtlasDataContext> open = () => new AtlasDataContext(options);
        services.AddSingleton(open);
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
        services.AddScoped<NotificationService>(s => new(s.GetRequiredService<INotificationRepository>(), []));
        services.AddScoped<IEventPublisher>(s =>
        {
            var publisher = new InMemoryEventPublisher();
            var content = new ObserveNodeLifecycleInContent(s.GetRequiredService<IDocumentRepository>());
            publisher.Subscribe<NodeCreatedV1>(content.Handle);
            publisher.Subscribe<NodeArchivedV1>(content.Handle);
            publisher.Subscribe<NotificationRequestedV1>(x => s.GetRequiredService<NotificationService>().Handle(x));
            return publisher;
        });
        services.AddScoped<IVotingEligibility, RepositoryVotingEligibility>();
        services.AddScoped<VoteMutationPolicy>(); services.AddScoped<CastVote>(); services.AddScoped<UndoVote>();
        services.AddScoped<NodeWorkflowService>(); services.AddScoped<NodeDescriptionService>();
        services.AddScoped<ReactionVotingService>(); services.AddScoped<CommentWorkflowService>();
        services.AddScoped<CommunityService>(); services.AddScoped<ModerationService>(); services.AddScoped<ModerationWorkflowService>();
        services.AddSingleton<DiscoveryCache>();
        services.AddScoped<IDiscoveryService>(s => new DiscoveryService(new SqlDiscoveryReadQuery(open, s.GetRequiredService<DiscoveryCache>())));
        services.AddScoped<RequestActor>();
        services.AddSingleton<ApiTelemetry>();
        services.AddHostedService<ApiAuditWorker>();
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
