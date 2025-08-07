using repository_pattern_experiment.Models.ViewModel;

namespace repository_pattern_experiment.Data.RepositoryPattern.IRepository
{

    /// <summary>
    /// Represents Cacheable User Information
    /// Ex: Author Info - rather than repeat it per post, have it once in the cache
    /// This will also make updates easier if the user updates information like their username/email
    /// </summary>
    public interface IAppUserRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Relates to <see cref="AppUser_ContentItem_ReadVM"/> which is a version meant for Author of a content item
        /// </remarks>
        Task<AppUser_ReadVM?> GetAppUser(Guid UserId);
    }
}
