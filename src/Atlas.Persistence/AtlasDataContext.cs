using Microsoft.EntityFrameworkCore;

namespace Atlas.Persistence;

public sealed class AtlasDataContext(DbContextOptions<AtlasDataContext> options) : DbContext(options)
{
    public DbSet<NodeRow> NodeRows => Set<NodeRow>();
    public DbSet<NodeTypeRow> NodeTypeRows => Set<NodeTypeRow>();
    public DbSet<DocumentRow> DocumentRows => Set<DocumentRow>();
    public DbSet<BlockRow> BlockRows => Set<BlockRow>();
    public DbSet<ParticipantRow> ParticipantRows => Set<ParticipantRow>();
    public DbSet<VoteRow> VoteRows => Set<VoteRow>();
    public DbSet<ReactionDefinitionRow> ReactionDefinitionRows => Set<ReactionDefinitionRow>();
    public DbSet<NodeReactionRow> NodeReactionRows => Set<NodeReactionRow>();
    public DbSet<CommunityRow> CommunityRows => Set<CommunityRow>();
    public DbSet<CommunityMembershipRow> CommunityMembershipRows => Set<CommunityMembershipRow>();
    public DbSet<CommunityNodeRow> CommunityNodeRows => Set<CommunityNodeRow>();
    public DbSet<CommentRow> CommentRows => Set<CommentRow>();
    public DbSet<ModerationCaseRow> ModerationCaseRows => Set<ModerationCaseRow>();
    public DbSet<NotificationRow> NotificationRows => Set<NotificationRow>();
    public DbSet<NotificationPreferencesRow> NotificationPreferencesRows => Set<NotificationPreferencesRow>();

    public static void PrepareRow(object row)
    {
        if (row is NodeReactionRow reaction)
            for (var position = 0; position < reaction.AuditHistory.Count; position++)
            { reaction.AuditHistory[position].NodeReactionId = reaction.Id; reaction.AuditHistory[position].Position = position; }
        if (row is NotificationRow notification)
            for (var position = 0; position < notification.DeliveryAttempts.Count; position++)
            { notification.DeliveryAttempts[position].NotificationId = notification.Id; notification.DeliveryAttempts[position].Position = position; }
    }

    protected override void OnModelCreating(ModelBuilder model) => ConfigureModel(model);

    internal static void ConfigureModel(ModelBuilder model)
    {
        model.HasAnnotation("ProductVersion", "10.0.0").HasAnnotation("Relational:MaxIdentifierLength", 128);
        model.UseIdentityColumns();
        model.Entity<NodeRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Title").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<Guid?>("DescriptionId").HasColumnType("uniqueidentifier");
            entity.Property<Guid?>("TypeId").HasColumnType("uniqueidentifier");
            entity.Property<Guid?>("AuthorId").HasColumnType("uniqueidentifier");
            entity.Property<string>("Status").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<NodeTypeRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Name").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<string>("Description").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string?>("OwnerId").HasColumnType("nvarchar(max)");
            entity.Property<bool>("IsSystemDefined").HasColumnType("bit").IsRequired();
            entity.Property<bool>("IsArchived").HasColumnType("bit").IsRequired();
            entity.Property<bool?>("AutoPluralize").HasColumnType("bit");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<DocumentRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<BlockRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Kind").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<string?>("Markdown").HasColumnType("nvarchar(max)");
            entity.Property<string?>("AltText").HasColumnType("nvarchar(max)");
            entity.Property<string?>("Caption").HasColumnType("nvarchar(max)");
            entity.Property<string?>("Url").HasColumnType("nvarchar(max)");
            entity.Property<string?>("Title").HasColumnType("nvarchar(max)");
            entity.Property<string?>("Description").HasColumnType("nvarchar(max)");
            entity.Property<Guid?>("ReferenceId").HasColumnType("uniqueidentifier");
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<ParticipantRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("DisplayName").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("Bio").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<bool>("IsActive").HasColumnType("bit").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<VoteRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("ParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("TargetId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("TargetType").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<int>("Value").HasColumnType("int").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<ReactionDefinitionRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Text").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("Emoji").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("Description").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("NormalizedText").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<Guid>("CreatedByParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<bool>("IsSuppressed").HasColumnType("bit").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<NodeReactionRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("NodeId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("ReactionDefinitionId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("AppliedByParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("LifecycleState").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<string>("Disposition").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset?>("RemovedAt").HasColumnType("datetimeoffset");
        });
        model.Entity<CommunityRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Name").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<string>("Description").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<Guid>("OwnerParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Status").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<CommunityMembershipRow>(entity =>
        {
            entity.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("ParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<DateTimeOffset>("JoinedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset?>("LeftAt").HasColumnType("datetimeoffset");
        });
        model.Entity<CommunityNodeRow>(entity =>
        {
            entity.Property<Guid>("CommunityId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("NodeId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("AssociatedByParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<DateTimeOffset>("AssociatedAt").HasColumnType("datetimeoffset").IsRequired();
        });
        model.Entity<CommentRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("TargetKind").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<Guid>("TargetId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid?>("ParentCommentId").HasColumnType("uniqueidentifier");
            entity.Property<Guid>("AuthorParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Body").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("Status").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset?>("RemovedAt").HasColumnType("datetimeoffset");
        });
        model.Entity<ModerationCaseRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("NodeId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("ReporterId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<string>("Reason").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string?>("Explanation").HasColumnType("nvarchar(max)");
            entity.Property<string>("ReportedTitle").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<int>("Status").HasColumnType("int").IsRequired();
            entity.Property<Guid?>("ReviewerId").HasColumnType("uniqueidentifier");
            entity.Property<string?>("DecisionReason").HasColumnType("nvarchar(max)");
            entity.Property<DateTimeOffset?>("DecidedAt").HasColumnType("datetimeoffset");
            entity.Property<int>("PublicReason").HasColumnType("int").IsRequired();
            entity.Property<DateTimeOffset?>("ReviewRequestedAt").HasColumnType("datetimeoffset");
            entity.Property<DateTimeOffset?>("VisibilityRestoredAt").HasColumnType("datetimeoffset");
            entity.Property<Guid?>("RestoredBy").HasColumnType("uniqueidentifier");
            entity.Property<string?>("RestorationReason").HasColumnType("nvarchar(max)");
        });
        model.Entity<NotificationRow>(entity =>
        {
            entity.Property<Guid>("Id").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("OccurrenceId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("RecipientParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Category").HasColumnType("int").IsRequired();
            entity.Property<bool>("InAppVisible").HasColumnType("bit").IsRequired();
            entity.Property<string>("Kind").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<string>("SubjectKind").HasColumnType("nvarchar(450)").HasMaxLength(450).IsRequired();
            entity.Property<Guid>("SubjectId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<Guid>("ActorParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<DateTimeOffset?>("ReadAt").HasColumnType("datetimeoffset");
            entity.Property<DateTimeOffset?>("DismissedAt").HasColumnType("datetimeoffset");
        });
        model.Entity<NotificationPreferencesRow>(entity =>
        {
            entity.Property<Guid>("ParticipantId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<bool>("DiscussionInApp").HasColumnType("bit").IsRequired();
            entity.Property<bool>("ModerationInApp").HasColumnType("bit").IsRequired();
            entity.Property<bool>("DiscussionEmail").HasColumnType("bit").IsRequired();
            entity.Property<bool>("ModerationEmail").HasColumnType("bit").IsRequired();
            entity.Property<bool>("DiscussionPush").HasColumnType("bit").IsRequired();
            entity.Property<bool>("ModerationPush").HasColumnType("bit").IsRequired();
        });
        model.Entity<ReactionAuditRow>(entity =>
        {
            entity.Property<Guid>("NodeReactionId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Position").HasColumnType("int").IsRequired();
            entity.Property<string>("Action").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<Guid?>("ActorParticipantId").HasColumnType("uniqueidentifier");
            entity.Property<DateTimeOffset>("OccurredAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<string>("LifecycleState").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<string>("Disposition").HasColumnType("nvarchar(max)").IsRequired();
            entity.Property<Guid?>("RelatedNodeReactionId").HasColumnType("uniqueidentifier");
        });
        model.Entity<DeliveryAttemptRow>(entity =>
        {
            entity.Property<Guid>("NotificationId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Position").HasColumnType("int").IsRequired();
            entity.Property<int>("Channel").HasColumnType("int").IsRequired();
            entity.Property<int>("Status").HasColumnType("int").IsRequired();
            entity.Property<DateTimeOffset>("AttemptedAt").HasColumnType("datetimeoffset").IsRequired();
            entity.Property<string?>("Error").HasColumnType("nvarchar(max)");
        });
        model.Entity<NodeParentRow>(entity =>
        {
            entity.Property<Guid>("NodeId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Position").HasColumnType("int").IsRequired();
            entity.Property<Guid>("ParentNodeId").HasColumnType("uniqueidentifier").IsRequired();
        });
        model.Entity<NodeRequestedTypeRow>(entity =>
        {
            entity.Property<Guid>("NodeId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Position").HasColumnType("int").IsRequired();
            entity.Property<Guid>("TypeId").HasColumnType("uniqueidentifier").IsRequired();
        });
        model.Entity<DocumentBlockRow>(entity =>
        {
            entity.Property<Guid>("DocumentId").HasColumnType("uniqueidentifier").IsRequired();
            entity.Property<int>("Position").HasColumnType("int").IsRequired();
            entity.Property<Guid>("BlockId").HasColumnType("uniqueidentifier").IsRequired();
        });
        model.Entity<NodeRow>(entity =>
        {
            entity.ToTable("NodeRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Title).HasMaxLength(450);
            entity.Ignore(row => row.RequestedSubNodeTypeIds);
            entity.HasMany(row => row.RequestedTypes).WithOne().HasForeignKey(row => row.NodeId).OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(row => row.ParentNodeIds);
            entity.HasMany(row => row.Parents).WithOne().HasForeignKey(row => row.NodeId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(row => row.Status).HasMaxLength(450);
        });
        model.Entity<NodeTypeRow>(entity =>
        {
            entity.ToTable("NodeTypeRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Name).HasMaxLength(450);
        });
        model.Entity<DocumentRow>(entity =>
        {
            entity.ToTable("DocumentRows");
            entity.HasKey(row => row.Id);
            entity.Ignore(row => row.BlockIds);
            entity.HasMany(row => row.Blocks).WithOne().HasForeignKey(row => row.DocumentId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<BlockRow>(entity =>
        {
            entity.ToTable("BlockRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Kind).HasMaxLength(450);
        });
        model.Entity<ParticipantRow>(entity =>
        {
            entity.ToTable("ParticipantRows");
            entity.HasKey(row => row.Id);
        });
        model.Entity<VoteRow>(entity =>
        {
            entity.ToTable("VoteRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.TargetType).HasMaxLength(450);
            entity.HasIndex(row => new { row.ParticipantId, row.TargetType, row.TargetId }).IsUnique().HasDatabaseName("IX_Vote_UniqueParticipantTarget");
        });
        model.Entity<ReactionDefinitionRow>(entity =>
        {
            entity.ToTable("ReactionDefinitionRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.NormalizedText).HasMaxLength(450);
        });
        model.Entity<NodeReactionRow>(entity =>
        {
            entity.ToTable("NodeReactionRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.LifecycleState).HasMaxLength(450);
            entity.HasMany(row => row.AuditHistory).WithOne().HasForeignKey(row => row.NodeReactionId).OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<CommunityRow>(entity =>
        {
            entity.ToTable("CommunityRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Name).HasMaxLength(450);
            entity.Property(row => row.Status).HasMaxLength(450);
            entity.HasIndex(row => row.Name).IsUnique().HasDatabaseName("IX_Community_Name");
        });
        model.Entity<CommunityMembershipRow>(entity =>
        {
            entity.ToTable("CommunityMembershipRows");
            entity.HasKey(row => new { row.CommunityId, row.ParticipantId });
        });
        model.Entity<CommunityNodeRow>(entity =>
        {
            entity.ToTable("CommunityNodeRows");
            entity.HasKey(row => new { row.CommunityId, row.NodeId });
        });
        model.Entity<CommentRow>(entity =>
        {
            entity.ToTable("CommentRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.TargetKind).HasMaxLength(450);
            entity.Property(row => row.Status).HasMaxLength(450);
        });
        model.Entity<ModerationCaseRow>(entity =>
        {
            entity.ToTable("ModerationCaseRows");
            entity.HasKey(row => row.Id);
        });
        model.Entity<NotificationRow>(entity =>
        {
            entity.ToTable("NotificationRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Kind).HasMaxLength(450);
            entity.Property(row => row.SubjectKind).HasMaxLength(450);
            entity.HasMany(row => row.DeliveryAttempts).WithOne().HasForeignKey(row => row.NotificationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(row => new { row.OccurrenceId, row.RecipientParticipantId }).IsUnique().HasDatabaseName("IX_Notification_UniqueOccurrenceRecipient");
        });
        model.Entity<NotificationPreferencesRow>(entity =>
        {
            entity.ToTable("NotificationPreferencesRows");
            entity.HasKey(row => row.ParticipantId);
        });
        model.Entity<NodeParentRow>(entity =>
        {
            entity.ToTable("NodeParents");
            entity.HasKey(row => new { row.NodeId, row.Position });
        });
        model.Entity<NodeRequestedTypeRow>(entity =>
        {
            entity.ToTable("NodeRequestedTypes");
            entity.HasKey(row => new { row.NodeId, row.Position });
        });
        model.Entity<DocumentBlockRow>(entity =>
        {
            entity.ToTable("DocumentBlocks");
            entity.HasKey(row => new { row.DocumentId, row.Position });
        });
        model.Entity<ReactionAuditRow>(entity =>
        {
            entity.ToTable("ReactionAuditEntries");
            entity.HasKey(row => new { row.NodeReactionId, row.Position });
        });
        model.Entity<DeliveryAttemptRow>(entity =>
        {
            entity.ToTable("NotificationDeliveryAttempts");
            entity.HasKey(row => new { row.NotificationId, row.Position });
        });
        foreach (var entity in model.Model.GetEntityTypes())
            foreach (var property in entity.FindPrimaryKey()!.Properties)
                property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
    }

}
