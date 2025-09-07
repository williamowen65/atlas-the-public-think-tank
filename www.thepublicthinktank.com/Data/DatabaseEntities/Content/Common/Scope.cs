using atlas_the_public_think_tank.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common
{
    /// <summary>
    /// Defines a scopes that can be defined on issues/solutions
    /// </summary>
    public class Scope
    {
        public Guid ScopeID { get; set; }
        public ICollection<Scale> Scales { get; set; } = new List<Scale>();
        public ICollection<Domain> Domains { get; set; } = new List<Domain>();
        public ICollection<EntityType> EntityTypes { get; set; } = new List<EntityType>();
        public ICollection<Timeframe> Timeframes { get; set; } = new List<Timeframe>();
        public ICollection<BoundaryType> Boundaries { get; set; } = new List<BoundaryType>();


    }



    public class ScopeModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>().ToTable(
                "Scopes", 
                "scopes",
                tb => tb.IsTemporal(temporal =>
                {
                    temporal.UseHistoryTable("ScopesHistory", "scopes");
                }));
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }
    }

    // NOTE I would still like to have the scope on the issue... But maybe not. 
    // It just needs to be versioned.

    




    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Scale
    {
        [EnumMember(Value = "Individual")]
        Individual = 1,

        [EnumMember(Value = "Household")]
        Household = 2,

        [EnumMember(Value = "Community")]
        Community = 3,

        [EnumMember(Value = "Regional")]
        Regional = 4,

        [EnumMember(Value = "National")]
        National = 5,

        [EnumMember(Value = "Global")]
        Global = 6
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Domain
    {
        [EnumMember(Value = "Political")]
        Political,

        [EnumMember(Value = "Economic")]
        Economic,

        [EnumMember(Value = "Cultural")]
        Cultural,

        [EnumMember(Value = "Environmental")]
        Environmental,

        [EnumMember(Value = "Technological")]
        Technological,

        [EnumMember(Value = "Health")]
        Health,

        [EnumMember(Value = "Social")]
        Social,

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EntityType
    {
        [EnumMember(Value = "Person")]
        Person,

        [EnumMember(Value = "Organization")]
        Organization,

        [EnumMember(Value = "Government")]
        Government,

        [EnumMember(Value = "Corporate")]
        Corporate,

        [EnumMember(Value = "Ecosystem")]
        Ecosystem,

        [EnumMember(Value = "Nonprofit")]
        Nonprofit,

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Timeframe
    {
        [EnumMember(Value = "Immediate")]
        Immediate,

        [EnumMember(Value = "Generational")]
        Generational,

        [EnumMember(Value = "LongTerm")]
        LongTerm,

        [EnumMember(Value = "ShortTerm")]
        ShortTerm
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BoundaryType
    {
        [EnumMember(Value = "Ecological")]
        Ecological,

        [EnumMember(Value = "Jurisdictional")]
        Jurisdictional,

        [EnumMember(Value = "Social")]
        Social,

        [EnumMember(Value = "Economic")]
        Economic,

        [EnumMember(Value = "Political")]
        Political
    }


}
