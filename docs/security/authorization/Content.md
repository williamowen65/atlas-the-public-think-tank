# Content Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Read node description/document | Public with owning node | No actor required for current public nodes | Read follows the owning node's public read path | Node/Content Console flow | Covered under current public-read policy |
| Create description for new node | Authenticated node author | Host derives author from authenticated Identity during node creation | Workflow creates Content for the node being authored; Content itself has no actor/owner | Node creation workflow | Host/workflow boundary |
| Add block | Node author | Host supplies authenticated Participant ID | Current Console calls `node.EnsureAuthoredBy(actorParticipantId)` before loading/managing the Content document | `DescriptionBlockCommands.Run` + Graph ownership | Covered at current workflow boundary |
| Update block | Node author | Same | Same Graph author check gates the entire description-management workflow; block validates content, not authorization | Description block workflow | Covered at current workflow boundary |
| Reorder block | Node author | Same | Same Graph author check; `Document.MoveBlock` enforces composition invariants only | Description block workflow | Covered at current workflow boundary |
| Remove block from document | Node author | Same | Same Graph author check; `Document.RemoveBlock` enforces composition invariants only | Description block workflow | Covered at current workflow boundary |
| Mutate Content directly without owning-node check | Not intended user workflow | N/A | Content aggregates do **not** carry author/owner identity | Content domain | **Boundary requirement:** future application/API workflow must preserve owning-node authorization |

## Review note

Content deliberately owns document composition rather than Graph authorship. Authorization therefore crosses a boundary: establish the authenticated actor, authorize against the owning Graph node, then mutate Content. Content's own methods protect structural/content invariants, not user identity.
