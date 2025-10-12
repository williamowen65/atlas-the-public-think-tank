using atlas_the_public_think_tank.Data.DatabaseEntities.Users;

namespace atlas_the_public_think_tank.Email.Models
{
    public class AccessibilityEmailModel
    {
        public required string Message { get; set; }

        public required AppUser? AppUser { get; set; } = null;
    }
}
