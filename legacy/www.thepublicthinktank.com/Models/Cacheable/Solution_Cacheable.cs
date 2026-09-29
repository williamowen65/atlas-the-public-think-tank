using atlas_the_public_think_tank.Models.Cacheable.Common;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution.SolutionVote;

namespace atlas_the_public_think_tank.Models.Cacheable
{
    /// <summary>
    /// ViewModel for the reading a solution
    /// </summary>
    public class Solution_Cacheable : ContentItem_Cacheable
    {
        public Guid SolutionID { get; set; }
        public Guid ParentIssueID { get; set; }


        public required SolutionVotes_Cacheable_ReadVM VoteStats { get; set; }

    }
}
