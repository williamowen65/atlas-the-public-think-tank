using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common
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
        public virtual AppUser? User { get; set; }
    }
}
