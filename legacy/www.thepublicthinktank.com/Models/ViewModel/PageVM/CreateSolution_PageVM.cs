using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class CreateSolution_PageVM : PageVM.Common.PageVM
    {
        public List<Scope> Scopes { get; set; } = new List<Scope>();

        /// <summary>
        /// Represents the main issue content that the user is creating
        /// </summary>
        public Solution_CreateOrEditVM Solution { get; set; } = new Solution_CreateOrEditVM();


    }
}
