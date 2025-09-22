using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class ConfirmVoteRequestDTO
    {
        public required Guid ContentID { get; set; }
        public required string ContentType { get; set; }
        public required int VoteValue { get; set; }
        //public ContentItem_ReadVM ContentItem { get; set; }
    }
    public class ConfirmVoteViewModel : ConfirmVoteRequestDTO
    {
        public ContentItem_ReadVM ContentItem { get; set; }
    }
}
