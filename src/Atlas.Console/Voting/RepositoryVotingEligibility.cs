using Atlas.Graph.Nodes;
using Atlas.Participants.Participants;
using Atlas.Voting.Eligibility;
using Atlas.Voting.Target;
using VotingParticipantId = Atlas.Voting.Votes.ParticipantId;
using ParticipantProfileId = Atlas.Participants.Participants.ParticipantId;

namespace Atlas.ConsoleApp.Voting
{
    /// <summary>
    /// Adapts Graph- and Participants-owned facts to Voting's
    /// identifier-only eligibility port.
    ///
    /// This adapter deliberately lives in the composition layer because
    /// answering these queries requires repositories and lifecycle concepts
    /// from multiple domains. It translates Voting IDs and returns facts; it
    /// does not decide whether a vote mutation is allowed. That decision stays
    /// in <see cref="VoteMutationPolicy"/> inside Voting.
    ///
    /// A future host or integration project may provide the same port without
    /// changing the Voting domain.
    /// </summary>
    public sealed class RepositoryVotingEligibility :
        IVotingEligibility
    {
        private readonly INodeRepository _nodes;
        private readonly IParticipantRepository _participants;

        public RepositoryVotingEligibility(
            INodeRepository nodes,
            IParticipantRepository participants)
        {
            _nodes = nodes;
            _participants = participants;
        }

        /// <summary>
        /// Treats an existing active participant selected by the Console
        /// as its prototype authenticated and eligible actor.
        /// </summary>
        public bool IsEligible(
            VotingParticipantId participantId)
        {
            var participant =
                _participants.GetById(
                    new ParticipantProfileId(
                        participantId.Id));

            return participant?.IsActive == true;
        }

        /// <summary>
        /// Resolves Node availability without exposing Graph entities
        /// to the Voting boundary.
        /// </summary>
        public bool IsAvailable(
            VoteTarget target)
        {
            if (target is not NodeVoteTarget)
            {
                return false;
            }

            var node =
                _nodes.GetById(
                    new NodeId(target.Id));

            return node?.Status == NodeStatus.Active;
        }
    }
}
