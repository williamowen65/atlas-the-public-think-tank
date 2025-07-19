using Microsoft.EntityFrameworkCore;
using System;
using atlas_the_public_think_tank.Models.Database;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedScopes
    {
        public SeedScopes(ModelBuilder modelBuilder)
        {
            // Seed Scopes
            modelBuilder.Entity<Scope>().HasData(
                new Scope { ScopeID = SeedIds.Scopes.Global, ScopeName = "Global" },
                new Scope { ScopeID = SeedIds.Scopes.National, ScopeName = "National" },
                new Scope { ScopeID = SeedIds.Scopes.Local, ScopeName = "Local" },
                new Scope { ScopeID = SeedIds.Scopes.Individual, ScopeName = "Individual" }
            );
        }
    }
}