using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    public class SolutionRepositoryViewModel
    {
        public Guid Id { get; set; }
        public Guid ParentIssueID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public Guid AuthorID { get; set; }

        public Scope Scope { get; set; }

        public ContentStatus ContentStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
    public interface ISolutionRepository
    {
        /// <returns>
        /// <see cref="SolutionSummaryViewModel"/> instance with populated navigation properties.
        /// <br/>
        /// It include the solution content <br/> 
        /// It does not include any nested content like Comments, Sub-issues
        /// </returns>
        Task<SolutionRepositoryViewModel?> GetSolutionById(Guid id);

        /// <summary>
        /// Adds a solution to an issue <br/>
        /// </summary>
        /// <param name="issueId"></param>
        /// <param name="solution"></param>
        /// <remarks>
        /// Updates related cache items (solution count on an issue)
        /// </remarks>
        /// <returns></returns>
        Task<Solution_ReadVM> AddSolutionAsync(Solution solution);

        Task<Solution_ReadVM> UpdateSolutionAsync(Solution solution);

        Task<int> GetSolutionVersionHistoryCount(Guid solutionID);

    }
}
