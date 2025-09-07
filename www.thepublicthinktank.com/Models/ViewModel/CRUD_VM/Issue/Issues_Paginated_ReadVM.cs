using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue
{
    /// <summary>
    /// Used to paginate issues for single issue or solution page
    /// </summary>
    public class Issues_Paginated_ReadVM
    {
            public List<Issue_ReadVM> Issues { get; set; } = new List<Issue_ReadVM>();
            public ContentCount_VM ContentCount { get; set; } = new ContentCount_VM();
            public int PageSize { get; set; }
            public int CurrentPage { get; set; }
    }
}
