# Architecture blackboards

These blackboards are exploratory visual companions to Atlas's authoritative
Markdown documentation.

- [PTT-68 current-state architecture](PTT-68-CURRENT-STATE.excalidraw) visualizes
  the implemented system, current interactions, boundary gaps, and candidate
  future domains discovered during PTT-68.
- [PTT-68 system blackboard](PTT-68-SYSTEM-BLACKBOARD.excalidraw) is the larger
  zoomable system graph. It places commands, use cases, event ports, current
  interactions, gaps, and candidate domains around their related boundaries.
- [The current-state context map](../architecture/CONTEXT-MAP.md) remains the
  authoritative inventory when the blackboard and written documentation differ.

## Legend

- Solid borders and arrows represent implemented components or interactions.
- Dashed borders and arrows represent candidates, gaps, or unresolved questions.
- Candidate-domain notes record discoveries only; they do not design or approve
  future boundaries.

Open an `.excalidraw` file in Excalidraw or a compatible editor. Keep related
areas on the same canvas unless the board becomes too crowded to review.

The current-state architecture board is a compact summary. The system
blackboard is intentionally much larger and is designed to be panned and
zoomed: boundary names remain visible from a distance, while ports, event
timing, and gap notes become readable when zoomed in.
