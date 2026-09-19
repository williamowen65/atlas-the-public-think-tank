# Architecture blackboards

These blackboards are exploratory visual companions to Atlas's authoritative
Markdown documentation.

- [Blackboard conventions](CONVENTIONS.md) describe the shared layout,
  editability, connection, traceability, and visual-language rules to apply to
  future workflow boards.

- [PTT-68 current-state architecture](PTT-68-CURRENT-STATE.excalidraw.png) visualizes
  the implemented system, current interactions, boundary gaps, and candidate
  future domains discovered during PTT-68.
- [PTT-68 system blackboard](SYSTEM-BLACKBOARD.excalidraw.png) is the larger
  zoomable system graph. It places commands, use cases, event ports, current
  interactions, gaps, and candidate domains around their related boundaries.
- [Reusable Node Tags](workflows/Node%20Tags/NodeTags.excalidraw.png) maps tag
  definition reuse, node-specific application, mutation authority, author
  disposition, lifecycle audit history, persistence, display, and the future
  Voting handoff.
- [Content Block Composition](workflows/Content/ContentBlocks.excalidraw.png) is a
  dashboard of Content ownership, block creation and validation, ordered document
  composition, type-specific payloads, JSON persistence, and reconstitution.
- [Node Tag Voting](workflows/Voting/NodeTagVoting.excalidraw.png) maps PTT-84's
  console voting interaction, NodeTag target and -1/+1 value types, cumulative
  score aggregation, cross-domain eligibility checks, JSON persistence, and
  verification tests.
- [The current-state context map](../architecture/CONTEXT-MAP.md) remains the
  authoritative inventory when the blackboard and written documentation differ.

## Legend

- Solid borders and arrows represent implemented components or interactions.
- Dashed borders and arrows represent candidates, gaps, or unresolved questions.
- Candidate-domain notes record discoveries only; they do not design or approve
  future boundaries.

Open an `.excalidraw.png` file for a mobile-friendly preview. Each PNG contains
embedded Excalidraw scene data, so it can also be imported into Excalidraw for
editing. The matching `.excalidraw` file remains the reviewable source. Keep
related areas on the same canvas unless the board becomes too crowded to
review.

The current-state architecture board is a compact summary. The system
blackboard is intentionally much larger and is designed to be panned and
zoomed: boundary names remain visible from a distance, while ports, event
timing, and gap notes become readable when zoomed in.
