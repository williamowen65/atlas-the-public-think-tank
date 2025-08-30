using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using atlas_the_public_think_tank.Models.ViewModel.UI_VM;

namespace atlas_the_public_think_tank.Models.ViewModel.PageVM
{
    public class CreateIssue_PageVM : PageVM.Common.PageVM
    {
        public List<Scope> Scopes { get; set; } = new List<Scope>();

        /// <summary>
        /// Represents the main issue content that the user is creating
        /// </summary>
        public Issue_CreateOrEditVM MainIssue { get; set; } = new Issue_CreateOrEditVM();

        /// <summary>
        /// When creating an issue, the user has the options to create solutions to that issue
        /// via the same interface. This is to help the user to not write out solutions inside the content for the issue.
        /// It's okay for this list to be empty. 
        /// </summary>
        /// <remarks>
        /// Special business logic: For any of the solutions to be published, their parents issue must be published
        /// </remarks>
        public List<Solution_CreateOrEditVM> Solutions { get; set; } = new List<Solution_CreateOrEditVM>();

     
    }
}
