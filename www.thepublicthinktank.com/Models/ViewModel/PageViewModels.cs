using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Models.ViewModel
{

    ///
    /// This file holds View models meant to be served to specific pages.
    ///

    /// <summary>
    /// This object defines info related to pagination
    /// </summary>
    public class PaginationStats
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents a view model for a pagination button, inheriting pagination statistics.
    /// </summary>
    /// <remarks>This class is used to encapsulate the data and behavior associated with a pagination button
    /// in a user interface, providing additional context or functionality related to pagination.</remarks>
    public class PaginationButton_ReadVM : PaginationStats
    { 
        public string ElementId { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
        //public ContentType ContentType { get; set; }
        public string ContentType { get; set; }
    }



    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    /// <remarks>
    /// This is used when serving the main index views for controllers <br/>
    /// In that scenario, ASP.NET Middleware has the responsibility to render the view <br/>
    /// and is given the data to do so.
    /// </remarks>
    public class PaginatedContentItemsResponse : PaginationStats
    {
        public List<ContentItem_ReadVM> ContentItems { get; set; }
    }
    public class PaginatedContentItemsJsonResponse
    {
        public string html { get; set; }

        public PaginationStats pagination{ get; set; }
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
