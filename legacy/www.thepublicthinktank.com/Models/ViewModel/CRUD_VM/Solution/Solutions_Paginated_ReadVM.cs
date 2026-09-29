using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution
{
    /// <summary>
    /// Used to paginate solutions for single issue's issue page
    /// </summary>
    public class Solutions_Paginated_ReadVM
    {
        public List<Solution_ReadVM> Solutions { get; set; } = new List<Solution_ReadVM>();

        public ContentCount_VM ContentCount { get; set; } = new ContentCount_VM();
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
