using atlas_the_public_think_tank.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common
{
    /// <summary>
    /// Defines a scopes that can be defined on issues/solutions
    /// </summary>
    public class Scope
    {
        public required Guid ScopeID { get; set; }
        public required string ScopeName { get; set; }

        //public required string ScopeDescription { get; set; }
       
        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<Issue.Issue>? Issues { get; set; }
    }

    public class ScopeModel : IModelComposer
    {
        public static void Declare(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scope>().ToTable("Scopes", "app");
        }
        public static void Build(ModelBuilder modelBuilder)
        {
            throw new NotImplementedException();
        }

    }
}
