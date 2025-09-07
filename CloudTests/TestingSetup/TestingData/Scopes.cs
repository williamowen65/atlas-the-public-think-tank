using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup.TestingData
{
    public static class Scopes
    {
        public static Scope GlobalScope { get; } = new Scope
        {
            ScopeID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Scales = { Scale.Global }
        };
        public static Scope CreateGlobalScope(ApplicationDbContext db)
        {
            db.Scopes.Add(GlobalScope);
            return GlobalScope;
        }

    }
}