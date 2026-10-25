using Atlas.Contracts.Observability;
using Atlas.Persistence.Observability;
using Atlas.Persistence.Discovery;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Atlas.Persistence.Identity;

namespace Atlas.Persistence;

// EF maps persistence rows rather than domain aggregates. Repositories translate between them,
// keeping SQL structure here and business behavior (ratings, permissions, lifecycle rules) in domains.
public sealed class AtlasDataContext(DbContextOptions<AtlasDataContext> options)
    : IdentityDbContext<AtlasIdentityUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ReportSubmissionQuotaRow> ReportSubmissionQuotaRows => Set<ReportSubmissionQuotaRow>();
    public DbSet<CommunityCreationQuotaRow> CommunityCreationQuotaRows => Set<CommunityCreationQuotaRow>();
    public DbSet<VoteActionQuotaRow> VoteActionQuotaRows => Set<VoteActionQuotaRow>();
    public DbSet<NodeCreationQuotaRow> NodeCreationQuotaRows => Set<NodeCreationQuotaRow>();
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

    /// <summary>Required by deployed hosts; local development acknowledges only its file destination.</summary>
    public bool RequireCloudAuditDelivery { get; set; }
    public DbSet<AuditRecordRow> AuditRecords => Set<AuditRecordRow>();
    public DbSet<AuditOutboxRow> AuditOutbox => Set<AuditOutboxRow>();

    // Completed writes are published only after the outer transaction commits. Failed writes
    // leave no success diagnostic; audit/outbox rows roll back with the business rows.
    private readonly List<(Guid TransactionId, TelemetryEvent Event)> _pendingTelemetry = [];
    /// <summary>Publish staged diagnostics for the transaction whose commit has been confirmed.</summary>
    /// <remarks>
    /// The developer/caller owns the ordering: first commit the SQL transaction successfully,
    /// then call this method with that transaction's ID. This method does not commit the
    /// transaction or verify its outcome; calling it early could report success for work
    /// that later rolls back. Do not call it after a failed or uncertain commit.
    /// SaveChanges handles this ordering for transactions it owns; an outer transaction
    /// owner must explicitly commit first and publish afterward.
    /// </remarks>
    public void PublishCommittedTelemetry(Guid transactionId)
    {
        foreach (var item in _pendingTelemetry.Where(item => item.TransactionId == transactionId))
            Telemetry.Record(item.Event);
        _pendingTelemetry.Clear();
    }

    /// <summary>Save business rows, selected audit evidence, delivery intent and revisions atomically.</summary>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        using var correlation = Telemetry.JoinOperation();
        var change = DiscoveryInvalidation.Changes(this);
        using var transaction = Database.CurrentTransaction is null ? Database.BeginTransaction() : null;
        // Select only approved transition metadata. Audit/outbox additions join this save;
        // no cloud or file write is attempted while the SQL transaction is open.
        var staged = AuditCapture.Stage(this);
        var committing = false;
        try
        {
            if (change.Page) AdvanceDiscovery(change.Detail);
            var count = base.SaveChanges(acceptAllChangesOnSuccess);
            var transactionId = Database.CurrentTransaction!.TransactionId;
            committing = transaction is not null;
            transaction?.Commit();
            _pendingTelemetry.AddRange(staged.Select(value => (transactionId, value)));
            // An outer operation will publish only after its own commit. Its pending
            // records are associated with that transaction ID, not a later retry's transaction.
            if (transaction is not null) PublishCommittedTelemetry(transactionId);
            return count;
        }
        catch (Exception error)
        {
            Telemetry.Failure(EventKind.StorageWrite, error,
                committing ? EventReason.CommitOutcomeUnknown : Telemetry.Reason(error));
            RemoveUncommittedAudit(staged);
            throw;
        }
    }

    /// <summary>Async equivalent: an outer transaction still owns its commit and telemetry publication.</summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // Classify before EF accepts tracked states, then join or create the transaction.
        using var correlation = Telemetry.JoinOperation();
        var change = DiscoveryInvalidation.Changes(this);
        await using var transaction = Database.CurrentTransaction is null
            ? await Database.BeginTransactionAsync(cancellationToken) : null;
        // Select only approved transition metadata. Audit/outbox additions join this save;
        // no cloud or file write is attempted while the SQL transaction is open.
        var staged = AuditCapture.Stage(this);
        var committing = false;
        try
        {
            if (change.Page)
            {
                var updated = await Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE DiscoveryRevisions SET PageVersion = PageVersion + 1, DetailVersion = DetailVersion + {(change.Detail ? 1 : 0)} WHERE Id = 1", cancellationToken);
                if (updated != 1) throw new InvalidOperationException("Discovery revision is missing; apply the pending migration.");
            }
            // Audit records/outbox are tracked additions, so EF inserts them in this same save.
            var count = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            var transactionId = Database.CurrentTransaction!.TransactionId;
            committing = transaction is not null;
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            _pendingTelemetry.AddRange(staged.Select(value => (transactionId, value)));
            // An outer operation will publish only after its own commit. Its pending
            // records are associated with that transaction ID, not a later retry's transaction.
            if (transaction is not null) PublishCommittedTelemetry(transactionId);
            return count;
        }
        catch (Exception error)
        {
            Telemetry.Failure(EventKind.StorageWrite, error,
                committing ? EventReason.CommitOutcomeUnknown : Telemetry.Reason(error));
            RemoveUncommittedAudit(staged);
            throw;
        }
    }

    private void RemoveUncommittedAudit(List<TelemetryEvent> staged)
    {
        var ids = staged.Select(value => value.EventId).ToHashSet();
        foreach (var entry in ChangeTracker.Entries().ToArray())
            if (entry.Entity is AuditRecordRow record && ids.Contains(record.Id)
                || entry.Entity is AuditOutboxRow delivery && ids.Contains(delivery.AuditRecordId))
                entry.State = EntityState.Detached;
    }

    private void AdvanceDiscovery(bool details)
    {
        if (Database.ExecuteSqlInterpolated($"UPDATE DiscoveryRevisions SET PageVersion = PageVersion + 1, DetailVersion = DetailVersion + {(details ? 1 : 0)} WHERE Id = 1") != 1)
            throw new InvalidOperationException("Discovery revision is missing; apply the pending migration.");
    }

    // Repository saves call this before tracking/saving a newly mapped row graph.
    // Audit entries and delivery attempts arrive as lists without database identities.
    // Each child needs its owner's ID (foreign key) and its zero-based list position;
    // together those values form its composite primary key in the tables below.
    // Example: reaction R with two entries becomes (R, 0) and (R, 1).
    // Position also lets repositories reconstruct list order; SQL rows have no implicit order.
    // This mutates the persistence children, not the domain object, and does not save anything.
    // Parent/type/block link setters in Rows.cs already supply these values when building links.
    public static void PrepareRow(object row)
    {
        if (row is NodeReactionRow reaction)
            for (var position = 0; position < reaction.AuditHistory.Count; position++)
            {
                reaction.AuditHistory[position].NodeReactionId = reaction.Id;
                reaction.AuditHistory[position].Position = position;
            }

        if (row is NotificationRow notification)
            for (var position = 0; position < notification.DeliveryAttempts.Count; position++)
            {
                notification.DeliveryAttempts[position].NotificationId = notification.Id;
                notification.DeliveryAttempts[position].Position = position;
            }
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.Entity<AuditRecordRow>(entity =>
        {
            entity.ToTable("AuditRecords");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever();
            entity.Property(row => row.EventName).HasMaxLength(100).IsRequired();
            entity.Property(row => row.PayloadJson).IsRequired();
            entity.HasIndex(row => row.ObservabilityOperationId);
            entity.HasIndex(row => new { row.TargetId, row.OccurredAtUtc });
        });
        model.Entity<AuditOutboxRow>(entity =>
        {
            entity.ToTable("AuditOutbox");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever();
            entity.Property(row => row.Destination).HasMaxLength(20).IsRequired();
            entity.HasIndex(row => new { row.AuditRecordId, row.Destination }).IsUnique();
            entity.HasIndex(row => new { row.Destination, row.DeliveredAtUtc, row.NextAttemptAtUtc });
            entity.HasOne<AuditRecordRow>().WithMany().HasForeignKey(row => row.AuditRecordId).OnDelete(DeleteBehavior.Restrict);
        });
        ConfigureModel(model);
        // One database-wide revision row lets readers detect stale page/card cache entries.
        // HasData records migration seed data; it does not insert a row on every context creation.
        model.Entity<DiscoveryRevisionRow>(entity =>
        {
            entity.ToTable("DiscoveryRevisions");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever();
            entity.HasData(new DiscoveryRevisionRow { Id = 1 });
        });
        // Support discovery aggregates and lookups by their target keys. Included columns
        // let SQL Server answer relevant reads from the index without fetching the base row.
        model.Entity<VoteRow>().HasIndex(row => new { row.TargetType, row.TargetId }).IncludeProperties(nameof(VoteRow.Value));
        // Equality prefix narrows to one sibling set; ordering keys support keyset seeks and TOP.
        model.Entity<CommentRow>().HasIndex(row => new { row.TargetKind, row.TargetId, row.ParentCommentId, row.CreatedAt, row.Id })
            .HasDatabaseName("IX_CommentRows_SiblingPage");
        model.Entity<NodeReactionRow>().HasIndex(row => new { row.NodeId, row.LifecycleState, row.ReactionDefinitionId });
        model.Entity<ModerationCaseRow>().HasIndex(row => new { row.NodeId, row.Status, row.VisibilityRestoredAt })
            .IncludeProperties(nameof(ModerationCaseRow.PublicReason), nameof(ModerationCaseRow.DecidedAt));
        IdentityModel.Configure(model);
        // Searchable text uses the shared case/accent-insensitive discovery collation.
        // This affects SQL comparisons; it does not rewrite stored text.
        model.Entity<NodeRow>().Property(row => row.Title).UseCollation(DiscoveryTextCollation.Name);
        model.Entity<BlockRow>().Property(row => row.Markdown).UseCollation(DiscoveryTextCollation.Name);
        model.Entity<BlockRow>().Property(row => row.AltText).UseCollation(DiscoveryTextCollation.Name);
        model.Entity<BlockRow>().Property(row => row.Caption).UseCollation(DiscoveryTextCollation.Name);
        model.Entity<BlockRow>().Property(row => row.Title).UseCollation(DiscoveryTextCollation.Name);
        model.Entity<BlockRow>().Property(row => row.Description).UseCollation(DiscoveryTextCollation.Name);

    }

    internal static void ConfigureModel(ModelBuilder model)
    {
        // Explicit provider metadata and column types make the intended SQL Server model visible.
        // These settings do not replace migration review when EF/provider versions change.
        model.HasAnnotation(
            "ProductVersion",
            "10.0.0"
        )
            .HasAnnotation(
                "Relational:MaxIdentifierLength",
                128
            );

        // Default strategy for generated numeric columns; primary-key generation is overridden below.
        // GUIDs are supplied by Atlas, and child positions are supplied by the row mapping.
        model.UseIdentityColumns();

        // Column shapes: uniqueidentifier for GUIDs, bit for booleans, datetimeoffset for timestamps.
        // IsRequired disallows SQL NULL; nullable references/timestamps permit an absent value.
        // nvarchar(max) holds longer text. Bounded nvarchar(450) supports index keys where used
        // and caps other labels; it is not a requirement that every bounded column be indexed.
        // String lengths also consume a composite index's total byte budget.
        // Ledger entries survive Node lifecycle changes; no cascading Node FK can refund allowance.
        model.Entity<ReportSubmissionQuotaRow>(entity =>
        {
            entity.ToTable("ReportSubmissionQuotaRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever().HasColumnType("uniqueidentifier");
            entity.Property(row => row.ParticipantId).HasColumnType("uniqueidentifier");
            entity.Property(row => row.OccurredAt).HasColumnType("datetimeoffset");
            entity.HasIndex(row => new { row.ParticipantId, row.OccurredAt });
        });
        model.Entity<CommunityCreationQuotaRow>(entity =>
        {
            entity.ToTable("CommunityCreationQuotaRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever().HasColumnType("uniqueidentifier");
            entity.Property(row => row.ParticipantId).HasColumnType("uniqueidentifier");
            entity.Property(row => row.OccurredAt).HasColumnType("datetimeoffset");
            entity.HasIndex(row => new { row.ParticipantId, row.OccurredAt });
        });
        model.Entity<VoteActionQuotaRow>(entity =>
        {
            entity.ToTable("VoteActionQuotaRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedNever().HasColumnType("uniqueidentifier");
            entity.Property(row => row.ParticipantId).HasColumnType("uniqueidentifier");
            entity.Property(row => row.OccurredAt).HasColumnType("datetimeoffset");
            entity.HasIndex(row => new { row.ParticipantId, row.OccurredAt });
        });
        model.Entity<NodeCreationQuotaRow>(entity =>
        {
            entity.ToTable("NodeCreationQuotaRows");
            entity.HasKey(row => row.NodeId);
            entity.Property(row => row.NodeId).ValueGeneratedNever().HasColumnType("uniqueidentifier");
            entity.Property(row => row.ParticipantId).HasColumnType("uniqueidentifier");
            entity.Property(row => row.CreatedAt).HasColumnType("datetimeoffset");
            entity.HasIndex(row => new { row.ParticipantId, row.CreatedAt });
        });
        model.Entity<NodeRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Title")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property(row => row.ShortDescription)
                .HasMaxLength(280)
                .HasDefaultValue("")
                .IsRequired();
            entity.Property<Guid?>("DescriptionId")
                .HasColumnType("uniqueidentifier");
            entity.Property<Guid?>("TypeId")
                .HasColumnType("uniqueidentifier");
            entity.Property<Guid?>("AuthorId")
                .HasColumnType("uniqueidentifier");
            entity.Property<string>("Status")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<NodeTypeRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Name")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<string>("Description")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string?>("OwnerId")
                .HasColumnType("nvarchar(max)");
            entity.Property<bool>("IsSystemDefined")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("IsArchived")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool?>("AutoPluralize")
                .HasColumnType("bit");
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<DocumentRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<BlockRow>(entity =>
        {
            // Persist only public searchable fields; SQL Server indexes this computed text.
            entity.Property<string>("SearchText")
                .HasComputedColumnSql(Discovery.DiscoveryFullTextSchema.BlockTextExpression, stored: true)
                .UseCollation(DiscoveryTextCollation.Name);
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Kind")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<string?>("Markdown")
                .HasColumnType("nvarchar(max)");
            entity.Property<string?>("AltText")
                .HasColumnType("nvarchar(max)");
            entity.Property<string?>("Caption")
                .HasColumnType("nvarchar(max)");
            entity.Property<string?>("Url")
                .HasColumnType("nvarchar(max)");
            entity.Property<string?>("Title")
                .HasColumnType("nvarchar(max)");
            entity.Property<string?>("Description")
                .HasColumnType("nvarchar(max)");
            entity.Property<Guid?>("ReferenceId")
                .HasColumnType("uniqueidentifier");
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<ParticipantRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("DisplayName")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("Bio")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<bool>("IsActive")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<VoteRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("ParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("TargetId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("TargetType")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<int>("Value")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<ReactionDefinitionRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Text")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("Emoji")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("Description")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("NormalizedText")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<Guid>("CreatedByParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<bool>("IsSuppressed")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<NodeReactionRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("NodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("ReactionDefinitionId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("AppliedByParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("LifecycleState")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<string>("Disposition")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset?>("RemovedAt")
                .HasColumnType("datetimeoffset");
        });
        model.Entity<CommunityRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Name")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<string>("Description")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<Guid>("OwnerParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Status")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<CommunityMembershipRow>(entity =>
        {
            entity.Property<Guid>("CommunityId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("ParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<DateTimeOffset>("JoinedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset?>("LeftAt")
                .HasColumnType("datetimeoffset");
        });
        model.Entity<CommunityNodeRow>(entity =>
        {
            entity.Property<Guid>("CommunityId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("NodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("AssociatedByParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<DateTimeOffset>("AssociatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
        });
        model.Entity<CommentRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("TargetKind")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<Guid>("TargetId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid?>("ParentCommentId")
                .HasColumnType("uniqueidentifier");
            entity.Property<Guid>("AuthorParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Body")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("Status")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset>("UpdatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset?>("RemovedAt")
                .HasColumnType("datetimeoffset");
        });
        model.Entity<ModerationCaseRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("NodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("ReporterId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<string>("Reason")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string?>("Explanation")
                .HasColumnType("nvarchar(max)");
            entity.Property<string>("ReportedTitle")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<int>("Status")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<Guid?>("ReviewerId")
                .HasColumnType("uniqueidentifier");
            entity.Property<string?>("DecisionReason")
                .HasColumnType("nvarchar(max)");
            entity.Property<DateTimeOffset?>("DecidedAt")
                .HasColumnType("datetimeoffset");
            entity.Property<int>("PublicReason")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<DateTimeOffset?>("ReviewRequestedAt")
                .HasColumnType("datetimeoffset");
            entity.Property<DateTimeOffset?>("VisibilityRestoredAt")
                .HasColumnType("datetimeoffset");
            entity.Property<Guid?>("RestoredBy")
                .HasColumnType("uniqueidentifier");
            entity.Property<string?>("RestorationReason")
                .HasColumnType("nvarchar(max)");
        });
        model.Entity<NotificationRow>(entity =>
        {
            entity.Property<Guid>("Id")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("OccurrenceId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("RecipientParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Category")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<bool>("InAppVisible")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<string>("Kind")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<string>("SubjectKind")
                .HasColumnType("nvarchar(450)")
                .HasMaxLength(450)
                .IsRequired();
            entity.Property<Guid>("SubjectId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<Guid>("ActorParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<DateTimeOffset>("CreatedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<DateTimeOffset?>("ReadAt")
                .HasColumnType("datetimeoffset");
            entity.Property<DateTimeOffset?>("DismissedAt")
                .HasColumnType("datetimeoffset");
        });
        model.Entity<NotificationPreferencesRow>(entity =>
        {
            entity.Property<Guid>("ParticipantId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<bool>("DiscussionInApp")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("ModerationInApp")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("DiscussionEmail")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("ModerationEmail")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("DiscussionPush")
                .HasColumnType("bit")
                .IsRequired();
            entity.Property<bool>("ModerationPush")
                .HasColumnType("bit")
                .IsRequired();
        });
        model.Entity<ReactionAuditRow>(entity =>
        {
            entity.Property<Guid>("NodeReactionId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Position")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<string>("Action")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<Guid?>("ActorParticipantId")
                .HasColumnType("uniqueidentifier");
            entity.Property<DateTimeOffset>("OccurredAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<string>("LifecycleState")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<string>("Disposition")
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property<Guid?>("RelatedNodeReactionId")
                .HasColumnType("uniqueidentifier");
        });
        model.Entity<DeliveryAttemptRow>(entity =>
        {
            entity.Property<Guid>("NotificationId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Position")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<int>("Channel")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<int>("Status")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<DateTimeOffset>("AttemptedAt")
                .HasColumnType("datetimeoffset")
                .IsRequired();
            entity.Property<string?>("Error")
                .HasColumnType("nvarchar(max)");
        });
        model.Entity<NodeParentRow>(entity =>
        {
            entity.Property<Guid>("NodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Position")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<Guid>("ParentNodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
        });
        model.Entity<NodeRequestedTypeRow>(entity =>
        {
            entity.Property<Guid>("NodeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Position")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<Guid>("TypeId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
        });
        model.Entity<DocumentBlockRow>(entity =>
        {
            entity.Property<Guid>("DocumentId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
            entity.Property<int>("Position")
                .HasColumnType("int")
                .IsRequired();
            entity.Property<Guid>("BlockId")
                .HasColumnType("uniqueidentifier")
                .IsRequired();
        });
        // Relational configuration follows the column shapes. Repeated Entity<T> calls configure
        // the same entity cumulatively; repeated identical length settings do not create columns.
        // ToTable fixes table names, and HasKey identifies each record uniquely.
        // Ignore excludes convenience ID lists; the HasMany collections persist their real child rows.
        // Cascade removes owned link/history rows on physical owner deletion, not on archival.
        // Supplemental search triggers require EF's trigger-compatible write path.
        model.Entity<NodeRow>(entity =>
        {
            entity.ToTable("NodeRows", table => table.UseSqlOutputClause(false));
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Title)
                .HasMaxLength(450);
            entity.Ignore(row => row.RequestedSubNodeTypeIds);
            entity.HasMany(row => row.RequestedTypes)
                .WithOne()
                .HasForeignKey(row => row.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(row => row.ParentNodeIds);
            entity.HasMany(row => row.Parents)
                .WithOne()
                .HasForeignKey(row => row.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(row => row.Status)
                .HasMaxLength(450);
        });
        model.Entity<NodeTypeRow>(entity =>
        {
            entity.ToTable("NodeTypeRows");
            entity.HasKey(row => row.Id);
            // Type names are query identifiers: case variants must not identify different types.
            entity.Property(row => row.Name)
                .HasMaxLength(450)
                .UseCollation("Latin1_General_100_CI_AS_SC");
            entity.HasIndex(row => row.Name).IsUnique();
        });
        model.Entity<DocumentRow>(entity =>
        {
            entity.ToTable("DocumentRows");
            entity.HasKey(row => row.Id);
            entity.Ignore(row => row.BlockIds);
            entity.HasMany(row => row.Blocks)
                .WithOne()
                .HasForeignKey(row => row.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<BlockRow>(entity =>
        {
            entity.ToTable("BlockRows", table => table.UseSqlOutputClause(false));
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Kind)
                .HasMaxLength(450);
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
            entity.Property(row => row.TargetType)
                .HasMaxLength(450);
            // Database uniqueness arbitrates concurrent first votes, even across multiple app instances.
            // Updates reuse the existing vote; target type distinguishes IDs from different target kinds.
            entity
                .HasIndex(row => new { row.ParticipantId, row.TargetType, row.TargetId })
                .IsUnique()
                .HasDatabaseName("IX_Vote_UniqueParticipantTarget");
        });
        model.Entity<ReactionDefinitionRow>(entity =>
        {
            entity.ToTable("ReactionDefinitionRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.NormalizedText)
                .HasMaxLength(450);
        });
        model.Entity<NodeReactionRow>(entity =>
        {
            entity.ToTable("NodeReactionRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.LifecycleState)
                .HasMaxLength(450);
            entity.HasMany(row => row.AuditHistory)
                .WithOne()
                .HasForeignKey(row => row.NodeReactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        model.Entity<CommunityRow>(entity =>
        {
            entity.ToTable("CommunityRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Name)
                .HasMaxLength(450);
            entity.Property(row => row.Status)
                .HasMaxLength(450);
            // Prevent duplicate community names even when two creates race.
            // Case/accent comparisons follow the database/column collation.
            entity
                .HasIndex(row => row.Name)
                .IsUnique()
                .HasDatabaseName("IX_Community_Name");
        });
        model.Entity<CommunityMembershipRow>(entity =>
        {
            entity.ToTable("CommunityMembershipRows");
            // The pair is the identity: one membership record per participant/community, including rejoining.
            entity.HasKey(row => new { row.CommunityId, row.ParticipantId });
        });
        model.Entity<CommunityNodeRow>(entity =>
        {
            entity.ToTable("CommunityNodeRows");
            // One association per community/node pair; no separate synthetic ID is needed.
            entity.HasKey(row => new { row.CommunityId, row.NodeId });
        });
        model.Entity<CommentRow>(entity =>
        {
            entity.ToTable("CommentRows", table => table.UseSqlOutputClause(false));
            entity.HasKey(row => row.Id);
            entity.Property(row => row.TargetKind)
                .HasMaxLength(450);
            entity.Property(row => row.Status)
                .HasMaxLength(450);
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
            entity.Property(row => row.Kind)
                .HasMaxLength(450);
            entity.Property(row => row.SubjectKind)
                .HasMaxLength(450);
            entity.HasMany(row => row.DeliveryAttempts)
                .WithOne()
                .HasForeignKey(row => row.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
            // Reprocessing one occurrence cannot insert a second notification for the same recipient.
            // This deduplicates notification rows, not external email/push delivery side effects.
            entity
                .HasIndex(row => new { row.OccurrenceId, row.RecipientParticipantId })
                .IsUnique()
                .HasDatabaseName("IX_Notification_UniqueOccurrenceRecipient");
        });
        model.Entity<NotificationPreferencesRow>(entity =>
        {
            entity.ToTable("NotificationPreferencesRows");
            // One preferences record per participant; the participant ID serves as both key and FK.
            entity.HasKey(row => row.ParticipantId);
        });
        // Ordered child identities are (owner ID, Position). Positions are unique within one owner,
        // but this alone does not guarantee contiguous numbering or unique referenced parent/block IDs.
        // Readers must order by Position; the database does not return insertion order automatically.
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
            entity.ToTable("DocumentBlocks", table => table.UseSqlOutputClause(false));
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
        // Atlas supplies its row key components: aggregate GUIDs, association pairs, and list positions.
        // Never tells EF to use the supplied value rather than expect database-generated keys.
        // Identity has its own key conventions (including generated numeric claim IDs).
        foreach (var entity in model.Model.GetEntityTypes()
                     .Where(entity => entity.ClrType.Namespace == typeof(ParticipantRow).Namespace))
            foreach (var property in entity.FindPrimaryKey()!.Properties)
                property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;

        // Foreign keys enforce existence of referenced records; indexes support lookups and joins.
        // HasOne<T>/WithMany() describe relationships without navigation properties on those rows.
        // Explicit constraint/index names keep migration output and SQL diagnostics recognizable.
        // NoAction blocks physical deletion while references exist; Atlas must resolve them explicitly.
        // This avoids deleting independent nodes, comments, votes, etc. when a participant is deleted.
        // Owned children above cascade instead: their rows are internal details of a deleted owner.
        // Nullable FKs allow NULL, but any non-null ID must reference an existing principal.
        //
        // Polymorphic Vote.TargetId, Comment.TargetId, and Notification.SubjectId can refer to
        // different tables depending on kind, so one ordinary FK cannot enforce those references.
        // Block.ReferenceId (poll/chart), event occurrence IDs, and opaque node-type owner strings
        // likewise have no mapped principal here. Application validation remains necessary.
        // FKs also do not enforce ancestry cycles, voting ranges, permissions, or archive rules.

        // A node references its description, type, and author without owning their physical deletion.
        model.Entity<NodeRow>()
            .HasOne<DocumentRow>()
            .WithMany()
            .HasForeignKey(row => row.DescriptionId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeRows_DocumentRows_DescriptionId");
        model.Entity<NodeRow>()
            .HasIndex(row => row.DescriptionId)
            .HasDatabaseName("IX_NodeRows_DescriptionId");
        model.Entity<NodeRow>()
            .HasOne<NodeTypeRow>()
            .WithMany()
            .HasForeignKey(row => row.TypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeRows_NodeTypeRows_TypeId");
        model.Entity<NodeRow>()
            .HasIndex(row => row.TypeId)
            .HasDatabaseName("IX_NodeRows_TypeId");
        model.Entity<NodeRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.AuthorId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeRows_ParticipantRows_AuthorId");
        model.Entity<NodeRow>()
            .HasIndex(row => row.AuthorId)
            .HasDatabaseName("IX_NodeRows_AuthorId");

        // Deleting a child node cascades its parent links; deleting a referenced parent is restricted.
        model.Entity<NodeParentRow>()
            .HasOne<NodeRow>()
            .WithMany()
            .HasForeignKey(row => row.ParentNodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeParents_NodeRows_ParentNodeId");
        model.Entity<NodeParentRow>()
            .HasIndex(row => row.ParentNodeId)
            .HasDatabaseName("IX_NodeParents_ParentNodeId");
        model.Entity<NodeRequestedTypeRow>()
            .HasOne<NodeTypeRow>()
            .WithMany()
            .HasForeignKey(row => row.TypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeRequestedTypes_NodeTypeRows_TypeId");
        model.Entity<NodeRequestedTypeRow>()
            .HasIndex(row => row.TypeId)
            .HasDatabaseName("IX_NodeRequestedTypes_TypeId");

        // Deleting a document removes its ordering links, while referenced block records are protected.
        model.Entity<DocumentBlockRow>()
            .HasOne<BlockRow>()
            .WithMany()
            .HasForeignKey(row => row.BlockId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_DocumentBlocks_BlockRows_BlockId");
        model.Entity<DocumentBlockRow>()
            .HasIndex(row => row.BlockId)
            .HasDatabaseName("IX_DocumentBlocks_BlockId");
        model.Entity<VoteRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_VoteRows_ParticipantRows_ParticipantId");
        model.Entity<VoteRow>()
            .HasIndex(row => row.ParticipantId)
            .HasDatabaseName("IX_VoteRows_ParticipantId");
        model.Entity<ReactionDefinitionRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.CreatedByParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ReactionDefinitionRows_ParticipantRows_CreatedByParticipantId");
        model.Entity<ReactionDefinitionRow>()
            .HasIndex(row => row.CreatedByParticipantId)
            .HasDatabaseName("IX_ReactionDefinitionRows_CreatedByParticipantId");
        model.Entity<NodeReactionRow>()
            .HasOne<NodeRow>()
            .WithMany()
            .HasForeignKey(row => row.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeReactionRows_NodeRows_NodeId");
        model.Entity<NodeReactionRow>()
            .HasIndex(row => row.NodeId)
            .HasDatabaseName("IX_NodeReactionRows_NodeId");
        model.Entity<NodeReactionRow>()
            .HasOne<ReactionDefinitionRow>()
            .WithMany()
            .HasForeignKey(row => row.ReactionDefinitionId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeReactionRows_ReactionDefinitionRows_ReactionDefinitionId");
        model.Entity<NodeReactionRow>()
            .HasIndex(row => row.ReactionDefinitionId)
            .HasDatabaseName("IX_NodeReactionRows_ReactionDefinitionId");
        model.Entity<NodeReactionRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.AppliedByParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NodeReactionRows_ParticipantRows_AppliedByParticipantId");
        model.Entity<NodeReactionRow>()
            .HasIndex(row => row.AppliedByParticipantId)
            .HasDatabaseName("IX_NodeReactionRows_AppliedByParticipantId");
        model.Entity<ReactionAuditRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ActorParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ReactionAuditEntries_ParticipantRows_ActorParticipantId");
        model.Entity<ReactionAuditRow>()
            .HasIndex(row => row.ActorParticipantId)
            .HasDatabaseName("IX_ReactionAuditEntries_ActorParticipantId");
        model.Entity<ReactionAuditRow>()
            .HasOne<NodeReactionRow>()
            .WithMany()
            .HasForeignKey(row => row.RelatedNodeReactionId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ReactionAuditEntries_NodeReactionRows_RelatedNodeReactionId");
        model.Entity<ReactionAuditRow>()
            .HasIndex(row => row.RelatedNodeReactionId)
            .HasDatabaseName("IX_ReactionAuditEntries_RelatedNodeReactionId");

        // Community owners, members, and associated nodes must exist; deletion is handled explicitly.
        model.Entity<CommunityRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.OwnerParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityRows_ParticipantRows_OwnerParticipantId");
        model.Entity<CommunityRow>()
            .HasIndex(row => row.OwnerParticipantId)
            .HasDatabaseName("IX_CommunityRows_OwnerParticipantId");
        model.Entity<CommunityMembershipRow>()
            .HasOne<CommunityRow>()
            .WithMany()
            .HasForeignKey(row => row.CommunityId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityMembershipRows_CommunityRows_CommunityId");
        model.Entity<CommunityMembershipRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityMembershipRows_ParticipantRows_ParticipantId");
        model.Entity<CommunityMembershipRow>()
            .HasIndex(row => row.ParticipantId)
            .HasDatabaseName("IX_CommunityMembershipRows_ParticipantId");
        model.Entity<CommunityNodeRow>()
            .HasOne<CommunityRow>()
            .WithMany()
            .HasForeignKey(row => row.CommunityId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityNodeRows_CommunityRows_CommunityId");
        model.Entity<CommunityNodeRow>()
            .HasOne<NodeRow>()
            .WithMany()
            .HasForeignKey(row => row.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityNodeRows_NodeRows_NodeId");
        model.Entity<CommunityNodeRow>()
            .HasIndex(row => row.NodeId)
            .HasDatabaseName("IX_CommunityNodeRows_NodeId");
        model.Entity<CommunityNodeRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.AssociatedByParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommunityNodeRows_ParticipantRows_AssociatedByParticipantId");
        model.Entity<CommunityNodeRow>()
            .HasIndex(row => row.AssociatedByParticipantId)
            .HasDatabaseName("IX_CommunityNodeRows_AssociatedByParticipantId");

        // Replies retain a real parent comment; physical parent deletion cannot silently remove the thread.
        model.Entity<CommentRow>()
            .HasOne<CommentRow>()
            .WithMany()
            .HasForeignKey(row => row.ParentCommentId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommentRows_CommentRows_ParentCommentId");
        model.Entity<CommentRow>()
            .HasIndex(row => row.ParentCommentId)
            .HasDatabaseName("IX_CommentRows_ParentCommentId");
        model.Entity<CommentRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.AuthorParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_CommentRows_ParticipantRows_AuthorParticipantId");
        model.Entity<CommentRow>()
            .HasIndex(row => row.AuthorParticipantId)
            .HasDatabaseName("IX_CommentRows_AuthorParticipantId");

        // Keep moderation references intact, including optional reviewer/restoration participants.
        model.Entity<ModerationCaseRow>()
            .HasOne<NodeRow>()
            .WithMany()
            .HasForeignKey(row => row.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ModerationCaseRows_NodeRows_NodeId");
        model.Entity<ModerationCaseRow>()
            .HasIndex(row => row.NodeId)
            .HasDatabaseName("IX_ModerationCaseRows_NodeId");
        model.Entity<ModerationCaseRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ReporterId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ModerationCaseRows_ParticipantRows_ReporterId");
        model.Entity<ModerationCaseRow>()
            .HasIndex(row => row.ReporterId)
            .HasDatabaseName("IX_ModerationCaseRows_ReporterId");
        model.Entity<ModerationCaseRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ModerationCaseRows_ParticipantRows_ReviewerId");
        model.Entity<ModerationCaseRow>()
            .HasIndex(row => row.ReviewerId)
            .HasDatabaseName("IX_ModerationCaseRows_ReviewerId");
        model.Entity<ModerationCaseRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.RestoredBy)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_ModerationCaseRows_ParticipantRows_RestoredBy");
        model.Entity<ModerationCaseRow>()
            .HasIndex(row => row.RestoredBy)
            .HasDatabaseName("IX_ModerationCaseRows_RestoredBy");

        // Notification recipient and actor reference participants; preferences share the participant key.
        model.Entity<NotificationRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.RecipientParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NotificationRows_ParticipantRows_RecipientParticipantId");
        model.Entity<NotificationRow>()
            .HasIndex(row => row.RecipientParticipantId)
            .HasDatabaseName("IX_NotificationRows_RecipientParticipantId");
        model.Entity<NotificationRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ActorParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NotificationRows_ParticipantRows_ActorParticipantId");
        model.Entity<NotificationRow>()
            .HasIndex(row => row.ActorParticipantId)
            .HasDatabaseName("IX_NotificationRows_ActorParticipantId");
        model.Entity<NotificationPreferencesRow>()
            .HasOne<ParticipantRow>()
            .WithMany()
            .HasForeignKey(row => row.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_NotificationPreferencesRows_ParticipantRows_ParticipantId");
    }
}
