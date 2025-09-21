namespace atlas_the_public_think_tank.Models.ViewModel.CRUD.User
{
    /// <summary>
    /// Represents info about the Author of a ContentItem
    /// </summary>
    /// <remarks>
    /// Hides personal information 
    /// and extra info that doesn't need to be cached per
    /// ContentItem Cache (Ex: SolutionVotes/commentVotes... 
    /// These can be found via a users profile instead)
    /// </remarks>
    public class AppUser_ReadVM
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string email { get; set; }

        public string FullName { get; set; } = "[Full Name]";
    }

    //public class AppUser_ReadVM_Extended : AppUser_ReadVM
    //{ 
    //    public 
    //}
}
