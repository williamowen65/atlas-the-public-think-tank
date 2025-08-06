using repository_pattern_experiment.Models.Database;
using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    public class IssueRepositoryViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        //public int SubIssueCount { get; set; }
        //public int CommentCount { get; set; }
        //public int SolutionCount { get; set; }
    }
    public interface IIssueRepository
    {

        /// <returns>
        /// <para>
        /// <see cref="IssueSummaryViewModel"/> instance with populated navigation properties.
        /// <br/>
        /// It includes the issue content <br/>
        /// It does not include any nested content like Solutions, Comments, Sub-issues
        /// </para>
        /// </returns>
        Task<IssueRepositoryViewModel?> GetIssueById(Guid id);

     
        /// <summary>
        /// Adds an issue (Root issue or Sub-Issue)
        /// </summary>
        /// <remarks>
        /// Potentially updates a related cache item (parent issue or parent solution count)
        /// </remarks>
        Task AddIssueAsync(Issue issue, Guid? parentIssueId, Guid? parentSolutionId);
    }
}
