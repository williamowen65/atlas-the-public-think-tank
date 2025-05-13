using atlas_the_public_think_tank.Data.SeedData;
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


    public DbSet<Issue> Issues { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<UserComment> Comments { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<BlockedContent> BlockedContents { get; set; }
    public DbSet<UserVote> UserVotes { get; set; }
    public DbSet<IssueVote> IssueVotes { get; set; }
    public DbSet<SolutionVote> SolutionVotes { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }
    public DbSet<IssueCategory> IssueCategories { get; set; }
    public DbSet<UserHistory> UserHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); // Ensure this is called

        // Configure schemas
        modelBuilder.Entity<Issue>().ToTable("Issues", "issues");
        modelBuilder.Entity<Solution>().ToTable("Solutions", "issues");
        modelBuilder.Entity<UserComment>().ToTable("Comments", "issues");
        modelBuilder.Entity<Category>().ToTable("Categories", "issues");
        modelBuilder.Entity<Scope>().ToTable("Scopes", "issues");
        modelBuilder.Entity<BlockedContent>().ToTable("BlockedContent", "issues");
        modelBuilder.Entity<UserVote>().ToTable("UserVotes", "issues");
        modelBuilder.Entity<IssueCategory>().ToTable("IssuesCategories", "issues");
        modelBuilder.Entity<UserHistory>().ToTable("UserHistory", "users");


        new SeedUsers(modelBuilder);

        new SeedCategories(modelBuilder);

        new SeedScopes(modelBuilder);

        new SeedIssues(modelBuilder);

        new SeedIssueCategories(modelBuilder);


        // Configure Issue entity
        modelBuilder.Entity<Issue>(entity =>
        {
            entity.HasKey(e => e.IssueID);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            // Self-referencing relationship
            entity.HasOne(e => e.ParentIssue)
                .WithMany(e => e.ChildIssues)
                .HasForeignKey(e => e.ParentIssueID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasOne(e => e.Author)
                .WithMany(e => e.Issues)
                .HasForeignKey(e => e.AuthorID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Scope)
                .WithMany(e => e.Issues)
                .HasForeignKey(e => e.ScopeID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BlockedContent)
                .WithMany(e => e.Issues)
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

            // Relationships
            entity.HasOne(e => e.Issue)
                .WithMany(e => e.Solutions)
                .HasForeignKey(e => e.IssueID)
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

            // Self-referencing relationship
            entity.HasOne(e => e.ParentComment)
                .WithMany(e => e.ChildComments)
                .HasForeignKey(e => e.ParentCommentID)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasOne(e => e.Issue)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.IssueID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.IssueSolutionID)
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

      


            // Configure the Vote hierarchy
            modelBuilder.Entity<UserVote>(entity =>
            {
                entity.HasKey(e => e.VoteID);

              
            });

        
            

        

        // Configure IssueCategory junction entity (composite key)
        modelBuilder.Entity<IssueCategory>(entity =>
        {
            entity.HasKey(e => new { e.CategoryID, e.IssueID });

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(e => e.IssueCategories)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Issue)
                .WithMany(e => e.IssueCategories)
                .HasForeignKey(e => e.IssueID)
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

            entity.HasOne(e => e.Issue)
                .WithMany()
                .HasForeignKey(e => e.IssueID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany()
                .HasForeignKey(e => e.IssueSolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Comment)
                .WithMany()
                .HasForeignKey(e => e.CommentID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
