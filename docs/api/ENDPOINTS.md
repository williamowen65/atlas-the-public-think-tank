# Endpoint and request index

[Request guide](HTTP-GUIDE.md) · [Workflow map](WORKFLOWS.md) · [Blackboard](../blackboards/workflows/Api/README.md)

All **70** controller method/route pairs have an example in the HTTP files.
This inventory is for navigation; success depends on account, ownership, role,
quota, visibility and resource state. The HTTP examples are manual documentation,
not a substitute for automated coverage. IDs in braces below are route parameters.

- Public GETs do not need a CSRF token. Protected GETs still need a session.
- Every POST/PUT/DELETE requires a valid CSRF pair, including anonymous account entry points.
- Except login/register/confirmation, writes require authentication; domain authorization still applies.
- Moderator-only operations include catalog reaction suppression and moderation queue/history/decision/restore.

## Accounts

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/auth/csrf` | [Csrf](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L21) | [auth.http](auth.http#L12) |
| POST | `/api/auth/login` | [Login](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L26) | [auth.http](auth.http#L32) |
| POST | `/api/auth/logout` | [Logout](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L35) | [auth.http](auth.http#L48) |
| GET | `/api/auth/session` | [Session](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L45) | [auth.http](auth.http#L44) |
| POST | `/api/auth/register` | [Register](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L48) | [auth.http](auth.http#L16) |
| POST | `/api/auth/confirm-email` | [Confirm](../../src/hosts/Atlas.Api/Controllers/AccountsController.cs#L66) | [auth.http](auth.http#L24) |

## Catalog

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/catalog/node-types` | [Types](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L17) | [workflows.http](workflows.http#L290) |
| POST | `/api/catalog/node-types` | [CreateType](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L19) | [workflows.http](workflows.http#L294) |
| PUT | `/api/catalog/node-types/{id}` | [UpdateType](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L21) | [workflows.http](workflows.http#L302) |
| POST | `/api/catalog/node-types/{id}/archive` | [Archive](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L31) | [workflows.http](workflows.http#L310) |
| GET | `/api/catalog/reactions` | [Reactions](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L38) | [workflows.http](workflows.http#L315) |
| POST | `/api/catalog/reactions` | [CreateReaction](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L40) | [workflows.http](workflows.http#L319) |
| POST | `/api/catalog/reactions/{id}/suppress` | [Suppress](../../src/hosts/Atlas.Api/Controllers/CatalogController.cs#L46) | [workflows.http](workflows.http#L327) |

## Comments

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/nodes/{node}/comments` | [Page](../../src/hosts/Atlas.Api/Controllers/CommentsController.cs#L17) | [workflows.http](workflows.http#L178) |
| POST | `/api/nodes/{node}/comments` | [Add](../../src/hosts/Atlas.Api/Controllers/CommentsController.cs#L28) | [workflows.http](workflows.http#L182) |
| POST | `/api/comments/{id}/replies` | [Reply](../../src/hosts/Atlas.Api/Controllers/CommentsController.cs#L31) | [workflows.http](workflows.http#L190) |
| PUT | `/api/comments/{id}` | [Edit](../../src/hosts/Atlas.Api/Controllers/CommentsController.cs#L34) | [workflows.http](workflows.http#L198) |
| DELETE | `/api/comments/{id}` | [Remove](../../src/hosts/Atlas.Api/Controllers/CommentsController.cs#L37) | [workflows.http](workflows.http#L206) |

## Communities

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/communities` | [Page](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L17) | [workflows.http](workflows.http#L221) |
| GET | `/api/communities/{id}` | [Get](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L23) | [workflows.http](workflows.http#L225) |
| GET | `/api/communities/{id}/nodes` | [Nodes](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L25) | [workflows.http](workflows.http#L229) |
| POST | `/api/communities` | [Create](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L33) | [workflows.http](workflows.http#L233) |
| PUT | `/api/communities/{id}/name` | [Rename](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L35) | [workflows.http](workflows.http#L241) |
| PUT | `/api/communities/{id}/description` | [Describe](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L37) | [workflows.http](workflows.http#L249) |
| POST | `/api/communities/{id}/archive` | [Archive](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L39) | [workflows.http](workflows.http#L257) |
| POST | `/api/communities/{id}/restore` | [Restore](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L41) | [workflows.http](workflows.http#L262) |
| PUT | `/api/communities/{id}/membership` | [Join](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L43) | [workflows.http](workflows.http#L267) |
| DELETE | `/api/communities/{id}/membership` | [Leave](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L45) | [workflows.http](workflows.http#L272) |
| PUT | `/api/communities/{id}/nodes/{node}` | [Associate](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L47) | [workflows.http](workflows.http#L277) |
| DELETE | `/api/communities/{id}/nodes/{node}` | [Disassociate](../../src/hosts/Atlas.Api/Controllers/CommunitiesController.cs#L49) | [workflows.http](workflows.http#L282) |

## Content

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/nodes/{node}/description` | [Read](../../src/hosts/Atlas.Api/Controllers/ContentController.cs#L20) | [workflows.http](workflows.http#L113) |
| POST | `/api/nodes/{node}/description/blocks` | [Add](../../src/hosts/Atlas.Api/Controllers/ContentController.cs#L30) | [workflows.http](workflows.http#L117) |
| PUT | `/api/nodes/{node}/description/blocks/{block}` | [Update](../../src/hosts/Atlas.Api/Controllers/ContentController.cs#L33) | [workflows.http](workflows.http#L125) |
| PUT | `/api/nodes/{node}/description/blocks/{block}/position` | [Move](../../src/hosts/Atlas.Api/Controllers/ContentController.cs#L36) | [workflows.http](workflows.http#L133) |
| DELETE | `/api/nodes/{node}/description/blocks/{block}` | [Remove](../../src/hosts/Atlas.Api/Controllers/ContentController.cs#L39) | [workflows.http](workflows.http#L141) |

## Moderation

| Method | Route | Action / source | Send request |
|---|---|---|---|
| POST | `/api/moderation/nodes/{id}/reports` | [Report](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L17) | [workflows.http](workflows.http#L354) |
| POST | `/api/moderation/nodes/{id}/review` | [Review](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L24) | [workflows.http](workflows.http#L362) |
| GET | `/api/moderation/queue` | [Queue](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L26) | [workflows.http](workflows.http#L367) |
| GET | `/api/moderation/nodes/{id}/history` | [History](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L28) | [workflows.http](workflows.http#L371) |
| POST | `/api/moderation/nodes/{id}/decision` | [Decide](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L30) | [workflows.http](workflows.http#L375) |
| POST | `/api/moderation/nodes/{id}/restore` | [Restore](../../src/hosts/Atlas.Api/Controllers/ModerationController.cs#L32) | [workflows.http](workflows.http#L383) |

## Navigation

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/nodes/{node}/children` | [Children](../../src/hosts/Atlas.Api/Controllers/NavigationController.cs#L20) | [workflows.http](workflows.http#L24) |
| GET | `/api/nodes/{node}/ancestors` | [Ancestors](../../src/hosts/Atlas.Api/Controllers/NavigationController.cs#L29) | [workflows.http](workflows.http#L28) |
| GET | `/api/nodes/{node}/votes` | [Votes](../../src/hosts/Atlas.Api/Controllers/NavigationController.cs#L37) | [workflows.http](workflows.http#L32) |
| GET | `/api/nodes/{node}/reactions` | [Reactions](../../src/hosts/Atlas.Api/Controllers/NavigationController.cs#L39) | [workflows.http](workflows.http#L36) |

## Nodes

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/nodes` | [Discover](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L23) | [workflows.http](workflows.http#L40) |
| GET | `/api/nodes/mine` | [Mine](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L30) | [workflows.http](workflows.http#L44) |
| GET | `/api/nodes/types` | [Types](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L34) | [workflows.http](workflows.http#L48) |
| GET | `/api/nodes/{id}` | [Get](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L37) | [workflows.http](workflows.http#L52) |
| POST | `/api/nodes` | [Create](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L40) | [workflows.http](workflows.http#L59) |
| PUT | `/api/nodes/{id}/title` | [Rename](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L49) | [workflows.http](workflows.http#L67) |
| POST | `/api/nodes/{id}/archive` | [Archive](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L52) | [workflows.http](workflows.http#L75) |
| POST | `/api/nodes/{id}/restore` | [Restore](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L55) | [workflows.http](workflows.http#L80) |
| PUT | `/api/nodes/{id}/parents/{parent}` | [Attach](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L58) | [workflows.http](workflows.http#L85) |
| DELETE | `/api/nodes/{id}/parents/{parent}` | [Detach](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L61) | [workflows.http](workflows.http#L90) |
| PUT | `/api/nodes/{id}/type/{type}` | [Type](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L64) | [workflows.http](workflows.http#L95) |
| PUT | `/api/nodes/{id}/requested-types/{type}` | [RequestType](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L67) | [workflows.http](workflows.http#L100) |
| DELETE | `/api/nodes/{id}/requested-types/{type}` | [WithdrawType](../../src/hosts/Atlas.Api/Controllers/NodesController.cs#L71) | [workflows.http](workflows.http#L105) |

## Notifications

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/notifications` | [Page](../../src/hosts/Atlas.Api/Controllers/NotificationsController.cs#L15) | [workflows.http](workflows.http#L394) |
| PUT | `/api/notifications/{id}/read` | [Read](../../src/hosts/Atlas.Api/Controllers/NotificationsController.cs#L19) | [workflows.http](workflows.http#L398) |
| DELETE | `/api/notifications/{id}` | [Dismiss](../../src/hosts/Atlas.Api/Controllers/NotificationsController.cs#L22) | [workflows.http](workflows.http#L403) |
| GET | `/api/notifications/preferences` | [Preferences](../../src/hosts/Atlas.Api/Controllers/NotificationsController.cs#L25) | [workflows.http](workflows.http#L408) |
| PUT | `/api/notifications/preferences` | [Preferences](../../src/hosts/Atlas.Api/Controllers/NotificationsController.cs#L28) | [workflows.http](workflows.http#L412) |

## Participants

| Method | Route | Action / source | Send request |
|---|---|---|---|
| GET | `/api/participants` | [Page](../../src/hosts/Atlas.Api/Controllers/ParticipantsController.cs#L16) | [workflows.http](workflows.http#L335) |
| GET | `/api/participants/{id}` | [Get](../../src/hosts/Atlas.Api/Controllers/ParticipantsController.cs#L22) | [workflows.http](workflows.http#L339) |
| PUT | `/api/participants/me` | [Update](../../src/hosts/Atlas.Api/Controllers/ParticipantsController.cs#L28) | [workflows.http](workflows.http#L343) |

## Votes

| Method | Route | Action / source | Send request |
|---|---|---|---|
| PUT | `/api/nodes/{node}/vote` | [Vote](../../src/hosts/Atlas.Api/Controllers/VotesController.cs#L20) | [workflows.http](workflows.http#L149) |
| DELETE | `/api/nodes/{node}/vote` | [Undo](../../src/hosts/Atlas.Api/Controllers/VotesController.cs#L23) | [workflows.http](workflows.http#L157) |
| PUT | `/api/nodes/{node}/reactions/vote` | [Reaction](../../src/hosts/Atlas.Api/Controllers/VotesController.cs#L26) | [workflows.http](workflows.http#L162) |
| DELETE | `/api/reactions/{association}/vote` | [UndoReaction](../../src/hosts/Atlas.Api/Controllers/VotesController.cs#L30) | [workflows.http](workflows.http#L170) |
