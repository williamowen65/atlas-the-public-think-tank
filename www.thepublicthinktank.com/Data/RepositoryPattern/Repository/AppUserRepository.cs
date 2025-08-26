using Microsoft.EntityFrameworkCore;
using atlas_the_public_think_tank.Data.RepositoryPattern.IRepository;
using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;

namespace atlas_the_public_think_tank.Data.RepositoryPattern.Repository
{
    public class AppUserRepository : IAppUserRepository
    {
        private readonly ApplicationDbContext _context;

        public AppUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Represents the entire AppUser minus sensitive info
        /// </summary>
        /// <remarks>
        /// Relates to <see cref="AppUser_ReadVM"/>
        /// </remarks>
        public async Task<AppUser_ReadVM?> GetAppUser(Guid UserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == UserId);

            if (user == null)
                return null;

            return new AppUser_ReadVM
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                email = user.Email ?? string.Empty
            };
        }
    }
}