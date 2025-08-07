using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    public interface IBreadcrumbRepository
    {
        Task<List<Breadcrumb_ReadVM>> GetBreadcrumbPagedAsync(Guid itemId);
    }
}

