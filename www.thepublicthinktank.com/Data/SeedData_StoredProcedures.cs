

using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;

namespace atlas_the_public_think_tank.Data
{
    public static partial class SeedData
    {

        public static async Task SeedDataMiscData(ApplicationDbContext context, UserManager<AppUser> userManager, IConfiguration config)
        {

         

            Console.WriteLine("Misc seed data added successfully");
        }

    }
}
