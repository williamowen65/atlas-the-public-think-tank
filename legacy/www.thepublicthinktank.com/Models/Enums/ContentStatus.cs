using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Enums
{
    /// <summary>
    /// Defines an enum for ContentStatus with string representations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentStatus
    {
        [EnumMember(Value = "Draft")]
        Draft,

        [EnumMember(Value = "Published")]
        Published,

        [EnumMember(Value = "Archived")]
        Archived
    }
}
