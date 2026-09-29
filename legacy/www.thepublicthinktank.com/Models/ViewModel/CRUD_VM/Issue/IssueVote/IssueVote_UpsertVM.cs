namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Issue.IssueVote
{
    public class IssueVote_UpsertVM
    {
        public required Guid IssueID { get; set; }
        public required int VoteValue { get; set; }
    }
}
