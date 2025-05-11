using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Models;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedScopes
    {
        public SeedScopes(ModelBuilder modelBuilder)
        {
            // Seed Scopes
            modelBuilder.Entity<Scope>().HasData(
                new Scope { ScopeID = 1, ScopeName = "Global" },
                new Scope { ScopeID = 2, ScopeName = "National" },
                new Scope { ScopeID = 3, ScopeName = "Local" },
                new Scope { ScopeID = 4, ScopeName = "Individual" }
            );
        }
    }
}
