using atlas_the_public_think_tank.Models.Database.Content.Comment;
using atlas_the_public_think_tank.Models.Database.Content.Issue;
using atlas_the_public_think_tank.Models.Database.Content.Solution;
using atlas_the_public_think_tank.Models.Database.History;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Database.Users
{
    /// <summary>
    /// Defines a user of this application
    /// </summary>
    public class AppUser : IdentityUser<Guid>
    {
        public AppUser()
        {
            Issues = new List<Issue>();
            Solutions = new List<Solution>();
            Comments = new List<Comment>();
            IssueVotes = new List<IssueVote>();
            SolutionVotes = new List<SolutionVote>();
            CommentVotes = new List<CommentVote>();
            UserHistory = new List<UserHistory>();
        }

        [JsonIgnore]
        public virtual ICollection<Issue> Issues { get; set; }
        [JsonIgnore]
        public virtual ICollection<Solution> Solutions { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; }
        [JsonIgnore]
        public virtual ICollection<IssueVote> IssueVotes { get; set; }
        public virtual ICollection<SolutionVote> SolutionVotes { get; set; }
        public virtual ICollection<CommentVote> CommentVotes { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserHistory> UserHistory { get; set; }
    }
}
