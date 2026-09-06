using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common
{


    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    /// <remarks>
    /// This is used when serving the main index views for controllers <br/>
    /// In that scenario, ASP.NET Middleware has the responsibility to render the view <br/>
    /// and is given the data to do so.
    /// </remarks>
    public class ContentItems_Paginated_ReadVM : PaginationStats_VM
    {
        public List<ContentItem_ReadVM> ContentItems { get; set; }
    }
  
}
