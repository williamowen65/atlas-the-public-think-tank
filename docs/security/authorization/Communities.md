# Communities Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Browse community](../../../src/Atlas.Communities/Communities/CommunityService.cs) | Public | No actor required | Public community read path | Community/Console behavior | Covered under current policy |
| [Create community](../../../src/Atlas.Communities/Communities/CommunityService.cs) | Authenticated participant | Host must derive owner Participant ID from authenticated Identity | `Community` requires non-empty owner, but cannot authenticate that owner ID | Community tests/current Console | Host identity required |
| [Rename community](../../../src/Atlas.Communities/Communities/Community.cs) | Owner | Host supplies authenticated Participant ID | `Community.Rename` → `EnsureOwner` | Community tests | Covered |
| [Change description](../../../src/Atlas.Communities/Communities/Community.cs) | Owner | Host supplies authenticated Participant ID | `Community.ChangeDescription` → `EnsureOwner` | Community tests | Covered |
| [Archive / restore](../../../src/Atlas.Communities/Communities/Community.cs) | Owner | Host supplies authenticated Participant ID | Community lifecycle methods → `EnsureOwner` | Community tests | Covered |
| [Join community](../../../src/Atlas.Communities/Communities/CommunityService.cs) | Participant joining self under current open-community policy | Host should derive Participant ID from authenticated Identity | `CommunityService.Join` checks community state but does not authenticate supplied participant ID | Community tests | Host identity required |
| [Leave community](../../../src/Atlas.Communities/Communities/CommunityService.cs) | Participant leaving self | Host should derive Participant ID from authenticated Identity | Service prevents owner leaving and requires membership, but cannot authenticate supplied participant ID | Community tests | Host identity required |
| [Associate node with community](../../../src/hosts/Atlas.Console/NodeCommands.cs) | Node author under current Console workflow | Host supplies authenticated Participant ID | Console's node workflow permits community management only for the node author; Communities enforces active-community state | Node author gate + Community tests | Covered at current workflow boundary; future API must preserve |

## Review note

Owner-managed community mutations have domain authorization. Create/join/leave rely on the host supplying the authenticated Participant ID. For node association, the current product behavior is now explicit: the **node author** may manage that node's community associations. The current Console enforces the Graph author gate before entering that workflow; a future API must preserve the same cross-boundary authorization before calling Communities.

## Creation budget is separate from authorization

An authenticated participant who may create a community must also have an available
creation slot: **one creation per rolling 24 hours**, enforced by Communities policy
and SQL persistence. Authentication/ownership checks remain necessary; a slot does
not grant permission to act as another participant. Renaming, editing, archiving,
restoring, joining/leaving and associating nodes do not consume creation slots.
See [policy, enforcement and migration handoff](../abuse-prevention/README.md#community-creation-budget)
and [quota regressions](../../../tests/Atlas.Persistence.Tests/SqlCommunityCreationQuotaTests.cs).
