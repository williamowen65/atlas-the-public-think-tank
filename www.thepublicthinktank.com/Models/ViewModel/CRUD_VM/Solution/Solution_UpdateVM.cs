using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution
{

    public class Solution_UpdateVM : Solution_CreateVM
    {
        /// <summary>
        /// ScopeId is Guid? with an DataAnnotation of Required 
        /// to intentionally throw the correct error response.
        /// This is because Guid is a non nullable type.
        /// </summary>
        [Required(ErrorMessage = "SolutionID is required when updating an existing solution")]
        public Guid? SolutionID { get; set; }
    }

}
