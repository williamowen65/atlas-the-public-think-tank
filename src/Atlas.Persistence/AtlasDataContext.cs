using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    protected override void OnModelCreating(ModelBuilder model) => ConfigureModel(model);

    internal static void ConfigureModel(ModelBuilder model)
    {
        model.Entity<NodeRow>(entity =>
        {
            entity.ToTable("NodeRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Title).HasMaxLength(450);
            entity.Property(row => row.RequestedSubNodeTypeIds).HasConversion(NullableSerializedListConverter<Guid>(), NullableSerializedListComparer<Guid>());
            entity.Property(row => row.ParentNodeIds).HasConversion(NullableSerializedListConverter<Guid>(), NullableSerializedListComparer<Guid>());
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
            entity.Property(row => row.BlockIds).HasConversion(SerializedListConverter<Guid>(), SerializedListComparer<Guid>());
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
            entity.HasIndex(row => new { row.ParticipantId, row.TargetType, row.TargetId }).IsUnique();
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
            entity.Property(row => row.AuditHistory).HasConversion(SerializedListConverter<ReactionAuditRow>(), SerializedListComparer<ReactionAuditRow>());
        });
        model.Entity<CommunityRow>(entity =>
        {
            entity.ToTable("CommunityRows");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Name).HasMaxLength(450);
            entity.Property(row => row.Status).HasMaxLength(450);
            entity.HasIndex(row => row.Name).IsUnique();
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
            entity.Property(row => row.DeliveryAttempts).HasConversion(SerializedListConverter<DeliveryAttemptRow>(), SerializedListComparer<DeliveryAttemptRow>());
            entity.HasIndex(row => new { row.OccurrenceId, row.RecipientParticipantId }).IsUnique();
        });
        model.Entity<NotificationPreferencesRow>(entity =>
        {
            entity.ToTable("NotificationPreferencesRows");
            entity.HasKey(row => row.ParticipantId);
        });
    }

    private static ValueConverter<List<T>, string> SerializedListConverter<T>() =>
        new(value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<T>>(value, (JsonSerializerOptions?)null) ?? new List<T>());
    private static ValueComparer<List<T>> SerializedListComparer<T>() =>
        new((a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            a => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null).GetHashCode(),
            a => JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(a, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new List<T>());
    private static ValueConverter<List<T>?, string?> NullableSerializedListConverter<T>() =>
        new(value => value == null ? null : JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => value == null ? null : JsonSerializer.Deserialize<List<T>>(value, (JsonSerializerOptions?)null));
    private static ValueComparer<List<T>?> NullableSerializedListComparer<T>() =>
        new((a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            a => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null).GetHashCode(),
            a => a == null ? null : JsonSerializer.Deserialize<List<T>>(JsonSerializer.Serialize(a, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null));
}
