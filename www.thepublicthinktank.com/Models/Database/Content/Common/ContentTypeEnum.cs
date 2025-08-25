using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Models.Database.Content.Common
{

    /// <summary>
    /// Defines an enum for ContentType with string representations
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContentTypeEnum
    {
        [EnumMember(Value = "Solution")]
        Solution,

        [EnumMember(Value = "Issue")]
        Issue,

        [EnumMember(Value = "Comment")]
        Comment
    }
}
