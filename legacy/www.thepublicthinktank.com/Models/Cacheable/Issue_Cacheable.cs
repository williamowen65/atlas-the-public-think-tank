using atlas_the_public_think_tank.Models.Cacheable.Common;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue.IssueVote;

namespace atlas_the_public_think_tank.Models.Cacheable
{
    /// <summary>
    /// Represents a Cacheable version of the Issue
    /// </summary>
    public class Issue_Cacheable : ContentItem_Cacheable
    {
        public Guid IssueID { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }
        public required IssueVotes_Cacheable_ReadVM VoteStats { get; set; }
    }
}
