# Communities Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Browse community | Public | No actor required | Public community read path | Community/Console behavior | Covered under current policy |
| Create community | Authenticated participant | Host must derive owner Participant ID from authenticated Identity | `Community` requires non-empty owner, but cannot authenticate that owner ID | Community tests/current Console | Host identity required |
| Rename community | Owner | Host supplies authenticated Participant ID | `Community.Rename` → `EnsureOwner` | Community tests | Covered |
| Change description | Owner | Host supplies authenticated Participant ID | `Community.ChangeDescription` → `EnsureOwner` | Community tests | Covered |
| Archive / restore | Owner | Host supplies authenticated Participant ID | Community lifecycle methods → `EnsureOwner` | Community tests | Covered |
| Join community | Participant joining self under current open-community policy | Host should derive Participant ID from authenticated Identity | `CommunityService.Join` checks community state but does not authenticate supplied participant ID | Community tests | Host identity required |
| Leave community | Participant leaving self | Host should derive Participant ID from authenticated Identity | Service prevents owner leaving and requires membership, but cannot authenticate supplied participant ID | Community tests | Host identity required |
| Associate node with community | Authenticated participant under current behavior | Host supplies Participant ID | Service checks community active; association records supplied participant ID | Community tests | **Review:** authorization policy may need clarification |

## Review note

Owner-managed community mutations have strong domain authorization. Create/join/leave rely more heavily on the host supplying the real authenticated Participant ID. Node association also needs a clear product authorization rule: the current service records who associated the node but does not establish which participants are permitted to do so.
