using repository_pattern_experiment.Data.SeedData.SeedIssues;
using repository_pattern_experiment.Data.SeedData;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Data.SeedData.SeedUsers;
using repository_pattern_experiment.Data.SeedData.SeedSolutions;
using repository_pattern_experiment.Data.SeedData.SeedVotes;

namespace repository_pattern_experiment.Data;

/// <summary>
/// Entity framework DB Context <br/>
/// Code first migrations
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
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
    public DbSet<IssueVote> IssueVotes { get; set; }
    public DbSet<SolutionVote> SolutionVotes { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }
    public DbSet<IssueCategory> IssueCategories { get; set; }
    public DbSet<SolutionCategory> SolutionCategories { get; set; }
    public DbSet<UserHistory> UserHistory { get; set; }

    /// <summary>
    /// FluentAPI code for code-first migration <br/>
    /// This code should be split into multiple files in future 
    /// as the DbSchema is about to get more complex once "rooms" are introduced
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); // Ensure this is called

        // Configure schemas
    
        modelBuilder.Entity<UserComment>().ToTable("Comments", "comments");
        modelBuilder.Entity<Category>().ToTable("Categories", "app");
        modelBuilder.Entity<Scope>().ToTable("Scopes", "app");
        modelBuilder.Entity<BlockedContent>().ToTable("BlockedContent", "app");
        modelBuilder.Entity<IssueVote>().ToTable("IssueVotes", "issues");
        modelBuilder.Entity<SolutionVote>().ToTable("SolutionVotes", "solutions");
        modelBuilder.Entity<CommentVote>().ToTable("CommentVotes", "comments");
        modelBuilder.Entity<IssueCategory>().ToTable("IssuesCategories", "issues");
        modelBuilder.Entity<SolutionCategory>().ToTable("SolutionsCategories", "solutions");
        modelBuilder.Entity<UserHistory>().ToTable("UserHistory", "users");



        new SeedUsers(modelBuilder);

        new SeedCategories(modelBuilder);

        new SeedScopes(modelBuilder);

        new SeedIssues(modelBuilder);

        new SeedIssueCategories(modelBuilder);

        new SeedSolutions(modelBuilder);

        new SeedIssueVotes(modelBuilder);

        new SeedSolutionVotes(modelBuilder);

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


            entity.HasOne(e => e.ParentSolution)
                .WithMany()
                .HasForeignKey(e => e.ParentSolutionID)
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

            entity.HasMany(e => e.ChildIssues)
              .WithOne(e => e.ParentSolution)
              .HasForeignKey(e => e.ParentSolutionID)
              .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasOne(e => e.ParentIssue)
                .WithMany(e => e.Solutions)
                .HasForeignKey(e => e.ParentIssueID)
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
                .HasForeignKey(e => e.SolutionID)
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



        // Configure IssueVote entity
        modelBuilder.Entity<IssueVote>(entity =>
        {
            entity.HasKey(e => e.VoteID);

            entity.HasOne(e => e.Issue)
                .WithMany(i => i.IssueVotes)
                .HasForeignKey(e => e.IssueID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SolutionVote entity
        modelBuilder.Entity<SolutionVote>(entity =>
        {
            entity.HasKey(e => e.VoteID);

            entity.HasOne(e => e.Solution)
                .WithMany(s => s.SolutionVotes)
                .HasForeignKey(e => e.SolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        //// Configure CommentVote entity
        modelBuilder.Entity<CommentVote>(entity =>
        {
            entity.HasKey(e => e.VoteID);

            entity.HasOne(e => e.Comment)
                .WithMany(c => c.CommentVotes)
                .HasForeignKey(e => e.CommentID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<SolutionCategory>(entity =>
        {
            entity.HasKey(e => new { e.CategoryID, e.SolutionID });

            // Relationships
            entity.HasOne(e => e.Category)
                .WithMany(e => e.SolutionCategories)
                .HasForeignKey(e => e.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Solution)
                .WithMany(e => e.SolutionCategories)
                .HasForeignKey(e => e.SolutionID)
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
                .HasForeignKey(e => e.SolutionID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Comment)
                .WithMany()
                .HasForeignKey(e => e.CommentID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    
    }
}
