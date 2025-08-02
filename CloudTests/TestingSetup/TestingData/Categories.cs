using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup.TestingData
{
    public static class Categories
    {

        public static Category CreateEnvironmentCategory(ApplicationDbContext db) {
            var category = new Category
            {
                CategoryID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CategoryName = "Environment"
            };
            db.Categories.Add(category);
            return category;

        }
    }
}
