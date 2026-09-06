using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common
{


    public class Breadcrumb_ReadVM
    {
        public string Title { get; set; }

        public Guid ContentID { get; set; }

        public ContentType ContentType { get; set; }

        public string Url
        {
            get
            {
                // Adjust base paths as needed for your Razor Pages routes
                return ContentType switch
                {
                    ContentType.Issue => $"/issue/{ContentID}",
                    ContentType.Solution => $"/solution/{ContentID}",
                    _ => "#"
                };
            }
        }
    }
}
