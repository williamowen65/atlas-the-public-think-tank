# Communities boundary

Communities owns public group identity, owner-controlled metadata and lifecycle, participant membership, and the organizational association between communities and nodes.

## Source of truth

| Concept | Communities owns |
|---|---|
| Community | ID, unique name, description, owner participant ID, active/archived status, timestamps |
| Membership | Community/participant pair, joined time, current active/left state |
| Node association | Community/node pair, associating participant, association time |

Participant and Node references are GUIDs. Communities does not contain Participant or Node objects.

## Invariants

- IDs and owner/participant/node references are non-empty GUIDs.
- Community names are unique without regard to case.
- The creator becomes owner and first active member.
- Only the owner may edit metadata or archive/restore the community.
- An owner cannot leave while still owning the community.
- Archived communities reject new memberships and node associations.
- One participant has at most one current membership record per community.
- One community/node pair has at most one association.
- Nodes may have zero, one, or many community associations.

## Boundary exclusions

Communities does not own Graph topology, node content, profiles, credentials, votes, system-wide moderation, or notifications. Node associations are discovery context and never create Graph parent relationships.

Private/restricted visibility, invitations, ownership transfer, removal of another participant, moderator appointment, community rules, and presentation customization are deferred. Moderator behavior belongs in the future Moderation boundary (PTT-101); future invitations may use Notifications (PTT-102).
