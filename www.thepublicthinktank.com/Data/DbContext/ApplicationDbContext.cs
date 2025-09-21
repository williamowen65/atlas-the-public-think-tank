using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using atlas_the_public_think_tank.Data.DatabaseEntities.Moderation;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.SeedData;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers;
using atlas_the_public_think_tank.Data.SeedData.SeedVotes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace atlas_the_public_think_tank.Data.DbContext;

/// <summary>
/// The files which define the models may also define
/// a ModelBuilder handler which configures the entity
/// with the FluentAPI
/// </summary>
public interface IModelComposer
{
    public abstract static void Declare(ModelBuilder modelBuilder);
    public abstract static void Build(ModelBuilder modelBuilder);
}

/// <summary>
/// Entity framework DB Context <br/>
/// Code first migrations
/// </summary>
public class ApplicationDbContext : UserHistoryDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }


    public DbSet<Issue> Issues { get; set; }
    public DbSet<Solution> Solutions { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<BlockedContent> BlockedContents { get; set; }
    public DbSet<IssueVote> IssueVotes { get; set; }
    public DbSet<IssueTag> IssueTags { get; set; }
    public DbSet<SolutionVote> SolutionVotes { get; set; }
    public DbSet<SolutionTag> SolutionTags { get; set; }
    public DbSet<CommentVote> CommentVotes { get; set; }
    

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
        CommentModel.Declare(modelBuilder);
        ScopeModel.Declare(modelBuilder);
        BlockedContentModel.Declare(modelBuilder);
        IssueModel.Declare(modelBuilder);
        IssueVoteModel.Declare(modelBuilder);
        SolutionModel.Declare(modelBuilder);
        SolutionVoteModel.Declare(modelBuilder);
        CommentVoteModel.Declare(modelBuilder);
        IssueTagModel.Declare(modelBuilder);
        SolutionTagModel.Declare(modelBuilder);
        UserHistoryModel.Declare(modelBuilder);

        IssueModel.Build(modelBuilder);
        SolutionModel.Build(modelBuilder);
        CommentModel.Build(modelBuilder);
        IssueVoteModel.Build(modelBuilder);
        SolutionVoteModel.Build(modelBuilder);
        CommentVoteModel.Build(modelBuilder);
        IssueTagModel.Build(modelBuilder);
        SolutionTagModel.Build(modelBuilder);
        UserHistoryModel.Build(modelBuilder);

    }


}
