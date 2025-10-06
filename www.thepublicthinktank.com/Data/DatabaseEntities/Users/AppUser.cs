using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Comment;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.History;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Users
{
    /// <summary>
    /// Defines a user of this application
    /// </summary>
    public class AppUser : IdentityUser<Guid>
    {

        /// <summary>
        /// Once an email is confirmed, the use will be subscribed to emails (see ConfirmEmail.cshtml.cs)
        /// </summary>
        public bool SubscribedToEmail { get; set; } = false;

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
        [JsonIgnore]
        public virtual ICollection<SolutionVote> SolutionVotes { get; set; }
        [JsonIgnore]
        public virtual ICollection<CommentVote> CommentVotes { get; set; }
        [JsonIgnore]
        public virtual ICollection<UserHistory> UserHistory { get; set; }

        [JsonIgnore]
        public virtual ICollection<EmailLog> EmailLog { get; set; }
    }
}
