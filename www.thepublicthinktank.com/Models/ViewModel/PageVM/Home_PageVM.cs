using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class Home_PageVM : PageVM.Common.PageVM
    {
        public ContentItems_Paginated_ReadVM PaginatedContent { get; set; } = new ContentItems_Paginated_ReadVM();

        
    }
}
