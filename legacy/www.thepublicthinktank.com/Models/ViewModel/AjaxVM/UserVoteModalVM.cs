using atlas_the_public_think_tank.Models.Cacheable;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.ContentItem_Common;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.User;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class UserVoteModalVM
    {
        public ContentItem_ReadVM content { get; set; }
        public Dictionary<AppUser_ReadVM, Vote_Cacheable> ContentVotesWithUserKey { set; get; }
    }
}
