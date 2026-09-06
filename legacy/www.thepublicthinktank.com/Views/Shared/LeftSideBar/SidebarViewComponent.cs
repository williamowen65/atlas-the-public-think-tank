using atlas_the_public_think_tank.Models.ViewModel.UI_VM;
using Microsoft.AspNetCore.Mvc;

namespace atlas_the_public_think_tank.Views.Shared.LeftSideBar
{
 
    /// <summary>
    /// The sidebar is served as a component b/c
    /// All pages have a sidebar, but not every page serves SideBar_VM content
    /// 
    /// This workflow makes it okay for the model to not contain that data.
    /// </summary>
    public class SidebarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(SideBar_VM? sidebarModel = null)
        {
            return View("~/Views/Shared/LeftSideBar/_Left-Sidebar-Toggle.cshtml", sidebarModel);
        }
    }
}
