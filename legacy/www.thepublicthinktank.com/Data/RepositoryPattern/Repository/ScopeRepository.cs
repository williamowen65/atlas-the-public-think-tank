using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class ScopeRepository : IScopeRepository
    {

        private ApplicationDbContext _context;
        public ScopeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Scope_ReadVM> AddScopeAsync(Scope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            // Add the scope to the context
            _context.Scopes.Add(scope);
            await _context.SaveChangesAsync();

            // Map to ViewModel
            var vm = new Scope_ReadVM
            {
                ScopeID = scope.ScopeID,
                Scales = scope.Scales,
                Domains = scope.Domains,
                EntityTypes = scope.EntityTypes,
                Timeframes = scope.Timeframes,
                Boundaries = scope.Boundaries
            };

            return vm;
        }

        public async Task<Scope_ReadVM?> UpdateScopeAsync(Scope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            var existingScope = await _context.Scopes.FindAsync(scope.ScopeID);
            if (existingScope == null)
                return null;

            // Update scalar and navigation properties
            _context.Entry(existingScope).CurrentValues.SetValues(scope);

            // For navigation collections, replace with new values
            existingScope.Scales = scope.Scales;
            existingScope.Domains = scope.Domains;
            existingScope.EntityTypes = scope.EntityTypes;
            existingScope.Timeframes = scope.Timeframes;
            existingScope.Boundaries = scope.Boundaries;

            await _context.SaveChangesAsync();

            var vm = new Scope_ReadVM
            {
                ScopeID = existingScope.ScopeID,
                Scales = existingScope.Scales,
                Domains = existingScope.Domains,
                EntityTypes = existingScope.EntityTypes,
                Timeframes = existingScope.Timeframes,
                Boundaries = existingScope.Boundaries
            };

            return vm;
        }
    }
}
