using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using System.ComponentModel.DataAnnotations;

namespace atlas_the_public_think_tank.Email.Models
{

    /// <remarks>
    /// Emails sent with InBoundEmailModel
    /// This email pattern does not user the EmailLogger because is not an email sent to the user. It is sent to atlas.thepublicthinktank.com + maybe developers
    /// </remarks>
    public class InBoundEmailModel
    {
        [Required]
        public string Message { get; set; }

        public AppUser? AppUser { get; set; } = null;

        public required string MessageType { get; set; }

        public List<IFormFile>? ImageAttachments { get; set; }

        public List<string> ImageCids { get; set; } 
    }
}
