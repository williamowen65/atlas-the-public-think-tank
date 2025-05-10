using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace atlas_the_public_think_tank.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<Forum> Forums { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<UserComment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<BlockedContent> BlockedContents { get; set; }
    public DbSet<UserVote> UserVotes { get; set; }
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<UserHistory> UserHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); // Ensure this is called

        // Configure schemas
        modelBuilder.Entity<Forum>().ToTable("Forums", "forums");
        modelBuilder.Entity<Solution>().ToTable("Solutions", "forums");
        modelBuilder.Entity<UserComment>().ToTable("Comments", "forums");
        modelBuilder.Entity<Category>().ToTable("Categories", "forums");
        modelBuilder.Entity<Scope>().ToTable("Scopes", "forums");
        modelBuilder.Entity<BlockedContent>().ToTable("BlockedContent", "forums");
        modelBuilder.Entity<UserVote>().ToTable("UserVotes", "forums");
        modelBuilder.Entity<ForumCategory>().ToTable("ForumsCategories", "forums");
        modelBuilder.Entity<UserHistory>().ToTable("UserHistory", "users");

        // Configure User entity
        //modelBuilder.Entity<User>(entity =>
        //{
        //});

        // Configure Forum entity
        modelBuilder.Entity<Forum>(entity =>
        {
            entity.HasKey(e => e.ForumID);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.Property(f => f.ForumID).ValueGeneratedNever();

            // Self-referencing relationship
            entity.HasOne(e => e.ParentForum)
                .WithMany(e => e.ChildForums)
                .HasForeignKey(e => e.ParentForumID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasOne(e => e.Author)
                .WithMany(e => e.Forums)
                .HasForeignKey(e => e.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Scope)
                .WithMany(e => e.Forums)
                .HasForeignKey(e => e.ScopeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BlockedContent)
                .WithMany(e => e.Forums)
                .HasForeignKey(e => e.BlockedContentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Solution entity
        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(e => e.SolutionID);
            entity.Property(e => e.Title).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentStatus).IsRequired();
            entity.Property(f => f.SolutionID).ValueGeneratedNever();

            // Relationships
            entity.HasOne(e => e.Forum)
                .WithMany(e => e.Solutions)
                .HasForeignKey(e => e.ForumID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Author)
                .WithMany(e => e.Solutions)
                .HasForeignKey(e => e.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BlockedContent)
                .WithMany(e => e.Solutions)
                .HasForeignKey(e => e.BlockedContentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Comment entity
        modelBuilder.Entity<UserComment>(entity =>
        {
            entity.HasKey(e => e.CommentID);
            entity.Property(e => e.Comment).HasMaxLength(3000).IsRequired();
            entity.Property(f => f.CommentID).ValueGeneratedNever();

            // Self-referencing relationship
            entity.HasOne(e => e.ParentComment)
                .WithMany(e => e.ChildComments)
                .HasForeignKey(e => e.ParentCommentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasOne(e => e.Forum)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.ForumID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.ForumSolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Author)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BlockedContent)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.BlockedContentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserVote entity (composite key)
        modelBuilder.Entity<UserVote>(entity =>
        {
            entity.HasKey(e => new { e.UserID, e.ForumID, e.ForumSolutionID, e.CommentID });
            entity.Property(e => e.Vote).IsRequired();

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(e => e.UserVotes)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Forum)
                .WithMany(e => e.UserVotes)
                .HasForeignKey(e => e.ForumID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany(e => e.UserVotes)
                .HasForeignKey(e => e.ForumSolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Comment)
                .WithMany(e => e.UserVotes)
                .HasForeignKey(e => e.CommentID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ForumCategory junction entity (composite key)
        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.HasKey(e => new { e.CategoryID, e.ForumID });

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(e => e.ForumCategories)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Forum)
                .WithMany(e => e.ForumCategories)
                .HasForeignKey(e => e.ForumID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserHistory entity
        modelBuilder.Entity<UserHistory>(entity =>
        {
            entity.HasKey(e => e.UserHistoryID);
            entity.Property(e => e.Action).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Link).HasMaxLength(200).IsRequired(false);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany(e => e.UserHistory)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Forum)
                .WithMany()
                .HasForeignKey(e => e.ForumID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany()
                .HasForeignKey(e => e.ForumSolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Comment)
                .WithMany()
                .HasForeignKey(e => e.CommentID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
