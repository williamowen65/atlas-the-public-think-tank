using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class ContentItems_Paginated_AjaxVM
    {
        public string html { get; set; }

        public PaginationStats_VM pagination { get; set; }

        /// <summary>
        /// This sidebar object contains potential updates
        /// to things like Page Info. 
        /// </summary>
        public SideBar_VM Sidebar { get; set; } = new SideBar_VM();

    }
}
