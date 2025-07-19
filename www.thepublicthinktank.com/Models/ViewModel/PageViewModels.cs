namespace atlas_the_public_think_tank.Models.ViewModel
{
 
    ///
    /// This file holds View models meant to be served to specific pages.
    ///



    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    public class PaginatedContentItemsResponse
    {
        public List<ContentItem_ReadVM> ContentItems { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }


    public class HomeIndexViewModel
    {

        public PaginatedContentItemsResponse PaginatedContent { get; set; } = new PaginatedContentItemsResponse();

        public SideBar_ReadVM Sidebar { get; set; } = new SideBar_ReadVM();

    }

    public class SideBar_ReadVM
    { 
    
    }
}
