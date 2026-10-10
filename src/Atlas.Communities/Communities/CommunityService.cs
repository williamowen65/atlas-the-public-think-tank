using Atlas.Communities.Memberships;
using Atlas.Communities.Nodes;

namespace Atlas.Communities.Communities;

/// <summary>Coordinates rules that require repository-wide Community state.</summary>
public sealed class CommunityService
{
    private readonly ICommunityRepository _communities;
    private readonly ICommunityMembershipRepository _memberships;
    private readonly ICommunityNodeRepository _nodes;

    public CommunityService(
        ICommunityRepository communities,
        ICommunityMembershipRepository memberships,
        ICommunityNodeRepository nodes)
    {
        _communities = communities;
        _memberships = memberships;
        _nodes = nodes;
    }

    public Community Create(string name, string description, Guid ownerParticipantId, DateTimeOffset createdAt)
    {
        return OperationBoundary.Execute(_communities, () =>
        {
            ReferenceValidation.Require(_communities, "Participant", ownerParticipantId);
            EnsureUniqueName(name);
            var community = new Community(name, description, ownerParticipantId, createdAt);
            _communities.Save(community);
            _memberships.Save(new CommunityMembership(community.Id, ownerParticipantId, createdAt));
            return community;
        });
    }

    public void Rename(Community community, Guid actorParticipantId, string name, DateTimeOffset changedAt)
    {
        OperationBoundary.Execute(_communities, () =>
        {
            ReferenceValidation.Require(_communities, "Community", community.Id.Value, false);
            ReferenceValidation.Require(_communities, "Participant", actorParticipantId);
            if (!string.Equals(community.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase)) EnsureUniqueName(name);
            community.Rename(actorParticipantId, name, changedAt);
            _communities.Save(community);
        });
    }

    public void Join(Community community, Guid participantId, DateTimeOffset joinedAt)
    {
        OperationBoundary.Execute(_communities, () =>
        {
            ReferenceValidation.Require(_communities, "Community", community.Id.Value);
            ReferenceValidation.Require(_communities, "Participant", participantId);
            EnsureActive(community);
            var membership = _memberships.Get(community.Id, participantId);
            if (membership is null) membership = new CommunityMembership(community.Id, participantId, joinedAt);
            else membership.Join(joinedAt);
            _memberships.Save(membership);
        });
    }

    public void Leave(Community community, Guid participantId, DateTimeOffset leftAt)
    {
        OperationBoundary.Execute(_communities, () =>
        {
            ReferenceValidation.Require(_communities, "Community", community.Id.Value, false);
            ReferenceValidation.Require(_communities, "Participant", participantId);
            if (community.OwnerParticipantId == participantId)
            {
                throw new InvalidOperationException("The owner cannot leave their community while they own it.");
            }

            var membership = _memberships.Get(community.Id, participantId)
                ?? throw new InvalidOperationException("The participant is not a member of this community.");
            membership.Leave(leftAt);
            _memberships.Save(membership);
        });
    }

    public void AssociateNode(Community community, Guid nodeId, Guid participantId, DateTimeOffset associatedAt)
    {
        OperationBoundary.Execute(_communities, () =>
        {
            ReferenceValidation.Require(_communities, "Community", community.Id.Value);
            ReferenceValidation.Require(_communities, "Participant", participantId);
            EnsureActive(community);
            ReferenceValidation.Require(_communities, "Node", nodeId);
            _nodes.Add(new CommunityNodeAssociation(community.Id, nodeId, participantId, associatedAt));
        });
    }

    private void EnsureUniqueName(string name)
    {
        if (_communities.GetAll().Any(existing => string.Equals(existing.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A community named '{name?.Trim()}' already exists.");
        }
    }

    private static void EnsureActive(Community community)
    {
        if (community.Status != CommunityStatus.Active)
        {
            throw new InvalidOperationException("Archived communities cannot accept new memberships or node associations.");
        }
    }
}
