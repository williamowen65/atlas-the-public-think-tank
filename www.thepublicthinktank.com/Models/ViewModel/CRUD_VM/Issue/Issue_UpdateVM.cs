using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue
{
    public class Issue_UpdateVM : Issue_CreateVM
    {
        /// <summary>
        /// ScopeId is Guid? with an DataAnnotation of Required 
        /// to intentionally throw the correct error response.
        /// This is because Guid is a non nullable type.
        /// </summary>
        [Required(ErrorMessage = "IssueID is required when updating an existing issue")]
        public Guid? IssueID { get; set; }
    }
}
