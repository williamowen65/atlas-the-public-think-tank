using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    /// <summary>
    /// Represents a cache unit of an issue
    /// </summary>
    public class IssueRepositoryViewModel
    {
        public Guid Id { get; set; }
        public Guid? ParentIssueID { get; set; }
        public Guid? ParentSolutionID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public Guid AuthorID { get; set; }

        //public AppUser_ContentItem_ReadVM Author { get; set; }

        public Scope Scope { get; set; }

        public ContentStatus ContentStatus { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        
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

        Task<List<ContentItem_ReadVM>?> GetIssueVersionHistoryById(Issue_ReadVM issue);

        /// <summary>
        /// Adds an issue (Root issue or Sub-Issue)
        /// </summary>
        /// <remarks>
        /// Potentially updates a related cache item (parent issue or parent solution count)
        /// </remarks>
        Task<Issue_ReadVM> AddIssueAsync(Issue issue);


        Task<Issue_ReadVM?> UpdateIssueAsync(Issue issue);

        Task<int> GetIssueVersionHistoryCount(Guid issueID);
    }
}
