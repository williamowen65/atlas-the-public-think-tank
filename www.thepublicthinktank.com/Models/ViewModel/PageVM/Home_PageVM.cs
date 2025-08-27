using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class Home_PageVM
    {
        public ContentItems_Paginated_ReadVM PaginatedContent { get; set; } = new ContentItems_Paginated_ReadVM();

        public SideBar_VM Sidebar { get; set; } = new SideBar_VM();
    }
}
