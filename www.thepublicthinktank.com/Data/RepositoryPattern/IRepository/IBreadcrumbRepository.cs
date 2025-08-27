using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.ContentItem_Common;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.IRepository
{

    public interface IBreadcrumbRepository
    {
        Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid? itemId);
    }
}

