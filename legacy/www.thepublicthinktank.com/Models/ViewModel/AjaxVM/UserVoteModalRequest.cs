using atlas_the_public_think_tank.Models.Enums;

namespace atlas_the_public_think_tank.Models.ViewModel.AjaxVM
{
    public class UserVoteModalRequest
    {
        public required Guid ContentId { get; set; }
        public required ContentType? ContentType { get; set; } = null;
    }
}
