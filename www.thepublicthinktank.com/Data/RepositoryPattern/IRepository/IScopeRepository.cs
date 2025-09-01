using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{
    public interface IScopeRepository
    {

        Task<Scope_ReadVM> AddScopeAsync(Scope scope);

        Task<Scope_ReadVM?> UpdateScopeAsync(Scope scope);

    }
}
