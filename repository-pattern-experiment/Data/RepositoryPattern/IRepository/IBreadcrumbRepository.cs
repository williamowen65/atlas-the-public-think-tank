namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{
    public class BreadcrumbItem_RepositoryVM
    { }
    public interface IBreadcrumbRepository
    {
        Task<List<BreadcrumbItem_RepositoryVM>> GetBreadcrumbAsync(Guid itemId);
    }
}
