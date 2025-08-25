using atlas_the_public_think_tank.Models.Database.Users;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Database.Content.Common
{


    public abstract class VoteBase
    {
        public required Guid VoteID { get; set; }
        public required Guid UserID { get; set; }
        public required int VoteValue { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Navigation property
        [JsonIgnore]
        public virtual required AppUser User { get; set; }
    }
}
