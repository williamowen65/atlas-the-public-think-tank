using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Enums
{

    /// <summary>
    /// Defines an enum for ContentType with string representations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentType
    {
        [EnumMember(Value = "Solution")]
        Solution,

        [EnumMember(Value = "Issue")]
        Issue,

        [EnumMember(Value = "Comment")]
        Comment
    }
}
