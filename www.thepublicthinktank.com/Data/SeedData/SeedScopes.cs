using Microsoft.EntityFrameworkCore;
using System;
 
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;

namespace atlas_the_public_think_tank.Data.SeedData
{
    public class SeedScopes
    {
        public SeedScopes(ModelBuilder modelBuilder)
        {


            // Update: Each seed datum is going to need to seed it's own scope data

            // Seed Scopes
            modelBuilder.Entity<Scope>().HasData(
                new Homelessness().scope,
                new MobileOutreachTeamsWithCliniciansAndSocialWorkers().scope
            );
        }
    }
}