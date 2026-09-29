using Atlas.Voting.Target;

namespace Atlas.Voting.Eligibility
{
    /// <summary>
    /// Supplies Voting with current interaction availability owned by
    /// the target's boundary.
    /// </summary>
    public interface IVoteTargetAvailability
    {
        /// <summary>
        /// Returns true when the referenced target currently permits
        /// vote mutations.
        /// </summary>
        bool IsAvailable(VoteTarget target);
    }
}
