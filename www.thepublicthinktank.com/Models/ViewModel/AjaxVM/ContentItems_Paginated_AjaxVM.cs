using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class ContentItems_Paginated_AjaxVM
    {
        public string html { get; set; }

        public PaginationStats_VM pagination { get; set; }

    }
}
