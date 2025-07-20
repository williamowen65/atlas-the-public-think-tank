using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.Database;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup.TestingData
{
    public static class Users
    {

        public static AppUser TestUser1 { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser@example.com",
            NormalizedUserName = "TESTUSER@EXAMPLE.COM",
            Email = "testuser@example.com",
            NormalizedEmail = "TESTUSER@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };

        public static AppUser CreateTestUser1(ApplicationDbContext db)
        {
          

            // Hash the password
            var passwordHasher = new PasswordHasher<AppUser>();
            TestUser1.PasswordHash = passwordHasher.HashPassword(TestUser1, "Password123!");

            db.Users.Add(TestUser1);

            return TestUser1;
        }
    }
}