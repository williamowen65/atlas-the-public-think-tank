# Architecture documentation refresh (PTT-151)

This pass is based on `main` after PR #360. Search phrase/strictness changes in [PR #362](https://github.com/williamowen65/atlas-the-public-think-tank/pull/362) are a separate review and are not described as merged here. [PTT-151](https://thepublicthinktank.atlassian.net/browse/PTT-151) tracks this documentation-only change.

## Current entry points

- [Application wiring](diagrams/WIRING.md): host/presentation ownership, composition, lifetimes, events and local diagnostics.
- [Application workflows](APPLICATION-WORKFLOWS.md): reusable use cases and backend boundaries.
- [API startup](../api/README.md): HTTP host, tests and linked security/Identity guides.
- [Frontend workbench](../../src/hosts/Atlas.Web/docs/STORYBOOK-WORKBENCH.md): shared UI, live versus isolated stories and launch commands.
- [Discovery reads](discovery-read-query.md): node feeds, snapshot reads, cache ownership and links to mixed search.

## Reviewed and corrected

| Area | Correction |
|---|---|
| Wiring and visual guide | Replaced the console-only composition class view with flowcharts. Added the implemented API and browser/Storybook presentation. Kept the detailed event class view and transaction sequences where their methods/relationships are useful. |
| Architecture/workflow indexes | Removed API/web-as-future-topology claims; linked the current ownership guide. |
| Discovery reads and walkthrough | Corrected serializable-read-lock descriptions to snapshots; documented actual API cache registration, full-text matching and the separate mixed-search path. Retained serializable **write** coordination where still correct. |
| Workbench guide | Replaced incremental prototype instructions that described live sessions, creation, comments and durable votes as future work. Linked owning guides instead of duplicating their behavior. |
| Ranking startup paragraph | Replaced disposable-database/test-connection instructions with the live local application connection and separate diagnostics host. |
| Observability setup | Removed the retired AddSecurityAuditOutbox migration recipe, pointed to committed baseline/setup guidance and acknowledged implemented API telemetry. Preserved the explicitly labeled pre-implementation rationale. |
| PR #360 review map | Marked it historical and pinned branch-relative GitHub source references to the merged PR's final head. It is review evidence, not current startup instructions. |
| Discovery performance guide | Corrected its summary of bounded EF ancestry reads and labeled the old verification run as historical evidence. Timings remain dated measurements, not present performance claims. |

## Maintenance and verification

Current guidance should name the owning code/configuration and link to one maintained setup or policy description. Remove superseded current instructions rather than appending another contradictory paragraph. Keep ADRs, named PR reviews and measured performance evidence as labeled history; do not silently rewrite them into current implementation claims.

The Discovery Excalidraw board remains historical PTT-132 material and is labeled accordingly in its walkthrough; its original drawing is not a current lock/search diagram. This pass does not claim an exhaustive audit of every requirements status, component-catalog prop or historical blackboard. Those should be checked against code when their area next changes.

Verification covers changed Markdown links/anchors, diagram structure and source references against Console Program, API Program/ApiServices/ApiBoundary, frontend configurations and discovery query code. Runtime code, schema, migrations and package dependencies are unchanged. Mermaid source is intended for GitHub rendering; no generated images are stored with this pass.
