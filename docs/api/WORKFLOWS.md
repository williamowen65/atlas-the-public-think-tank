# Console / HTTP workflow map

The API reuses application transactions, topology coordination, events and durable
quotas. Navigation menus become resource URLs; session state becomes an Identity
cookie. Frontend controls and developer-only database inspection are not API parity
requirements. This map describes exposed operations, not full automated coverage.

| Console capability | HTTP route family | Reused owner |
|---|---|---|
| Register / confirm / sign in / sign out | `/api/auth/*` | AtlasAccounts, UserManager, SignInManager |
| Discover/filter/page nodes | `GET /api/nodes`, `/api/nodes/mine` | DiscoveryQuery / DiscoveryService |
| Read node, children, ancestors | `/api/nodes/{id}`, `/children`, `/ancestors` | Graph, AncestorNavigation; hidden placeholders |
| Create / rename / archive / restore | `/api/nodes`, `/{id}/title`, `/archive`, `/restore` | NodeWorkflowService |
| Change type / request child types | `/{id}/type/{type}`, `/requested-types/{type}` | NodeWorkflowService / Graph |
| Attach / detach parent | `/{id}/parents/{parent}` | NodeWorkflowService / topology boundary |
| Read / add / update / reorder / remove description blocks | `/{id}/description` and `/blocks` | NodeDescriptionService / Content |
| Custom node types / edit / archive | `/api/catalog/node-types` | Graph type rules / NodeTypeCreationService |
| Reactions / vote / undo | `/api/catalog/reactions`, `/api/nodes/{id}/reactions`, `/api/reactions/{association}/vote` | Graph and ReactionVotingService / Voting |
| Node importance cast / change / undo / summary | `/api/nodes/{id}/vote`, `/votes` | CastVote / UndoVote / GetVoteSummary |
| Community create / edit / lifecycle / join / leave / link nodes | `/api/communities` | CommunityService |
| Participant browse / profile / self-edit | `/api/participants`, `/me` | Participants / UpdateParticipantProfile |
| Comment roots / direct replies / edit / remove | `/api/nodes/{id}/comments`, `/api/comments/{id}` | CommentWorkflowService; keyset cursor pair |
| Report / request review / queue / decision / restore / history | `/api/moderation` | ModerationService / ModerationWorkflowService |
| In-app feed / read / dismiss / preferences | `/api/notifications` | NotificationService |
| Local diagnostic/audit inspection | Development files only | ApiTelemetry / ApiAuditWorker |

## Review the critical differences

- Inputs contain operation data; an authenticated participant is established by the
  host. No operation accepts a client-supplied acting participant or privilege flag.
- HTTP success is emitted after the same application method returns; multi-save
  transactions and after-commit event semantics remain in the application layer.
- JSON blocks use an allowlisted kind; no domain events, tracked rows or internal
  delivery errors are serialized as API resources.
- Removed comments expose placeholders, not retained bodies. Hidden node content
  is omitted from public detail/description/discovery reads.
- Community association removal currently requires its owner in the API because
  the reusable service has no independently enforced removal permission policy.
- Comments have bounded SQL sibling pages. Existing catalog/list repositories
  still materialize collections; response pagination alone does not bound DB work.
