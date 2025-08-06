using repository_pattern_experiment.Models.Database;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    public class SolutionRepositoryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SubIssueCount { get; set; }
        public int CommentCount { get; set; }
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
        Task AddSolutionAsync(Solution solution, Guid parentIssueId);

    }
}
