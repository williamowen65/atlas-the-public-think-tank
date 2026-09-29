namespace atlas_the_public_think_tank.Models.ViewModel.CRUD_VM.Solution.SolutionVote
{
    public class SolutionVote_UpsertVM
    {
        public required Guid SolutionID { get; set; }
        public required int VoteValue { get; set; }
    }
}
