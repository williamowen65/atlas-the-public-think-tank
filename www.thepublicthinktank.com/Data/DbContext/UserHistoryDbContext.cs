using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;
using Solution = atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution.Solution;

namespace atlas_the_public_think_tank.Data.DbContext
{
    public class UserHistoryDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        private readonly IServiceProvider _serviceProvider;

        public UserHistoryDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public DbSet<UserHistory> UserHistory { get; set; }

        public override int SaveChanges()
        {
            AddUserHistoryEntriesSync();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            await AddUserHistoryEntriesAsync();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // Synchronous version for SaveChanges
        private void AddUserHistoryEntriesSync()
        {
            var historyEntries = new List<UserHistory>();

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added ||
                                     e.State == EntityState.Modified ||
                                     e.State == EntityState.Deleted))
            {
                if (entry.Entity is UserHistory) continue;
                if (entry.Entity is Scope) continue;

                using var scope = _serviceProvider.CreateScope();
                var services = scope.ServiceProvider;

                var userHistoryProcessor = services.GetService<UserHistoryProcessor>();

                // Call the async method synchronously (safe only if no awaits inside)
                var historyTask = userHistoryProcessor!.CreateHistoryEntry(entry);
                historyTask.Wait();
                var history = historyTask.Result;
                if (history != null)
                {
                    historyEntries.Add(history);
                }
            }

            UserHistory.AddRange(historyEntries);

            
        }

        // Async version for SaveChangesAsync
        private async Task AddUserHistoryEntriesAsync()
        {
            var historyEntries = new List<UserHistory>();

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added ||
                                     e.State == EntityState.Modified ||
                                     e.State == EntityState.Deleted))
            {
                if (entry.Entity is UserHistory) continue;
                if (entry.Entity is Scope) continue;

                using var scope = _serviceProvider.CreateScope();
                var services = scope.ServiceProvider;

                var userHistoryProcessor = services.GetService<UserHistoryProcessor>();

                var history = await userHistoryProcessor.CreateHistoryEntry(entry);
                if (history != null)
                {
                    historyEntries.Add(history);
                }
            }

            UserHistory.AddRange(historyEntries);
        }
    }

    /// <summary>
    /// Added to DI for access to other classes
    /// </summary>
    public class UserHistoryProcessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserHistoryProcessor(IHttpContextAccessor httpContextAccessor) {
            _httpContextAccessor = httpContextAccessor;
        
        }

        public async Task<UserHistory?> CreateHistoryEntry(EntityEntry entry)
        {
            var now = DateTime.UtcNow;

            HttpContext? httpContext = _httpContextAccessor.HttpContext;

            Guid? userId = null;

            UserHistory? userHistory = null;

            if (entry.Entity is AppUser && entry.State == EntityState.Added)
            {
                userId = ((AppUser)entry.Entity).Id;
                userHistory = new UserHistory
                {
                    Action = "Account Created",
                    UserID = (Guid)userId!,
                    Timestamp = now
                };
            }

            if (entry.Entity is IssueVote)
            {
                userId = ExtractUserId(entry, ((IssueVote)entry.Entity).UserID);

                Guid issueId = ((IssueVote)entry.Entity).IssueID;
                var issue = await Read.Issue(issueId, new ContentFilter());

                int voteValue = ((IssueVote)entry.Entity).VoteValue;

                string actionText = VoteSwitch(voteValue, entry);

                userHistory = new UserHistory
                {
                    Action = $"{actionText} on an issue: {issue!.Title}",
                    UserID = (Guid)userId!,
                    Timestamp = now,
                    IssueID = issueId
                };
            }

            if (entry.Entity is SolutionVote)
            {
                userId = ExtractUserId(entry, ((SolutionVote)entry.Entity).UserID);

                Guid solutionId = ((SolutionVote)entry.Entity).SolutionID;
                var solution = await Read.Solution(solutionId, new ContentFilter());

                int voteValue = ((SolutionVote)entry.Entity).VoteValue;

                string actionText = VoteSwitch(voteValue, entry);

                userHistory = new UserHistory
                {
                    Action = $"{actionText} on a solution: {solution!.Title}",
                    UserID = (Guid)userId!,
                    Timestamp = now,
                    SolutionID = solutionId
                };
            }

            if (entry.Entity is Issue)
            {
                Issue thisIssue = ((Issue)entry.Entity);
                userId = ExtractUserId(entry, thisIssue.AuthorID);

                string actionText = ContentItemSwitch(entry, "an issue");

                userHistory = new UserHistory
                {
                    Action = $"{actionText}: {thisIssue.Title}",
                    UserID = (Guid)userId!,
                    Timestamp = now,
                    IssueID = thisIssue.IssueID
                };

            }

            if (entry.Entity is Solution)
            {
                Solution thisSolution = ((Solution)entry.Entity);
                userId = ExtractUserId(entry, thisSolution.AuthorID);

                string actionText = ContentItemSwitch(entry, "a solution");

                userHistory = new UserHistory
                {
                    Action = $"{actionText}: {thisSolution.Title}",
                    UserID = (Guid)userId!,
                    Timestamp = now,
                    SolutionID = thisSolution.SolutionID
                };

            }

            CacheHelper.ClearUserHistoryCache((Guid)userId!);

            return userHistory;
           
        }

        public Guid? ExtractUserId(EntityEntry entry, Guid fallbackGuid)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            Guid? userId = null;
            if (httpContext != null)
            {
                var userIdClaim = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var _userId)) return null;
                userId = _userId;
            }
            else
            {
                userId = fallbackGuid;
            }

            return userId;
        }

        /// <summary>
        /// Language = "an issue" or "a solution"
        /// </summary>
        public string ContentItemSwitch(EntityEntry entry, string language)
        {
            string actionText;

            switch (entry.State)
            {
                case EntityState.Added:
                    actionText = $"Created {language}";
                    break;
                case EntityState.Modified:
                    actionText = $"Updated {language}";
                    break;
                case EntityState.Deleted:
                    actionText = $"Deleted {language}";
                    break;
                default:
                    actionText = $"Interacted with {language}";
                    break;
            }

            return actionText;
        }

        public string VoteSwitch(int voteValue, EntityEntry entry)
        {

            string actionText;
            switch (entry.State)
            {
                case EntityState.Added:
                    actionText = $"Voted {voteValue}";
                    break;
                case EntityState.Modified:
                    actionText = $"Updated a vote to {voteValue}";
                    break;
                case EntityState.Deleted:
                    actionText = "Removed a vote";
                    break;
                default:
                    actionText = "Voted";
                    break;
            }

            return actionText;
        }


    }
}
