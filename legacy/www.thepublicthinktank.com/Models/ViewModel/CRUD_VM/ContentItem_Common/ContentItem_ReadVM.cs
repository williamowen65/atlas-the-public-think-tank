using atlas_the_public_think_tank.Models.Cacheable.Common;
using atlas_the_public_think_tank.Models.Enums;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common.ContentItemVote;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common
{

    public class ContentItem_ReadVM : ContentItem_Cacheable
    {
        public Guid ContentID { get; set; }
        public required ContentItemVotes_ReadVM VoteStats { get; set; }
        public ContentType ContentType { get; set; }
        public Issues_Paginated_ReadVM PaginatedSubIssues { get; set; } = new Issues_Paginated_ReadVM();
        public Solutions_Paginated_ReadVM? PaginatedSolutions { get; set; } = new Solutions_Paginated_ReadVM();
    }
}
