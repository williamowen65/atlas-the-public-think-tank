using atlas_the_public_think_tank.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.Common
{
    public class ContentItem_CreateVM_EditVM
    {
        [Display(Name = "Title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(150, MinimumLength = 15, ErrorMessage = "Title must be between 15 and 150 characters")]
        public string Title { get; set; }

        [Display(Name = "Body text")]
        [MinLength(30, ErrorMessage = "Content must be at least 30 characters")]
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }


        /// <summary>
        /// ScopeId is Guid? with an DataAnnotation of Required 
        /// to intentionally throw the correct error response.
        /// This is because Guid is a non nullable type.
        /// </summary>
        [Required(ErrorMessage = "Scope is required")]
        [Display(Name = "Scope")]
        public Guid? ScopeID { get; set; }


        public ContentStatus? ContentStatus { get; set; }
    }


}
