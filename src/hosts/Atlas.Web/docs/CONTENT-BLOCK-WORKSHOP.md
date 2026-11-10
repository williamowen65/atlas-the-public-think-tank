# Content blocks: November 9 workshop and first implementation

Tracking: [PTT-160](https://thepublicthinktank.atlassian.net/browse/PTT-160). The complete product discussion, decisions, unresolved choices, and acceptance criteria are recorded on that Jira task. The separate-short-description discovery idea [PTTD-38](https://thepublicthinktank.atlassian.net/browse/PTTD-38) was superseded.

## Product model

The node **title** and **type** are separate from the node's **ordered content blocks**. A short-description textarea is not part of the authoring experience. The full node shows the complete ordered content. A feed card shows only a **bounded preview**, respecting the familiar dial and mobile card footprint.

A block-based node must be easy to author: a single vertical sequence of blocks, not an unrestricted grid. Initial controls support Text (safe Markdown) and Image (HTTP(S) URL, required alternative text, optional caption); each block has add/edit/remove and keyboard-operable move up/down. Existing unsupported block kinds are preserved when reordering or saving, and are not presented as working polls/charts.

The model should allow future plugin-like block renderers with a full-node view, compact preview, editor, validation and versioned payload. Possible later kinds include video, link preview, evidence, poll, chart and live external data. Additional permission and security policy must precede executing external integrations.

## First implementation in this branch

- `BlockComposer.svelte`: shared sequential text/image authoring UI for creation and existing node editing, including safe Markdown preview, image metadata, move buttons and add/remove controls.
- `contentAuthoring.ts`: authoring model, type-dependent mutation payloads, validations and derived legacy excerpt. Unsupported content survives as original payload.
- `NodeForm.svelte` and `LiveNodeForm.svelte`: one body editor in the existing node-card-shaped creation form, with browser-private draft state. The backend creation endpoint creates one Markdown block; the client inserts the desired ordered blocks with existing authenticated APIs, then removes that initial block. It retains a pending node ID on failure to prevent creating a duplicate on an explicit retry.
- `NodeEditControl.svelte`: load node and ordered blocks; author-checked publishing of changed, added, deleted and reordered blocks through the existing Content API. A saved browser revision is private. Saving multiple changes is **not currently atomic** across requests; the editor tells the author a partial save may have occurred on failure.
- `NodePage.svelte`: render the Content document as the complete node body alongside the dial, rather than repeating the shortDescription.
- `FeedBlockPreview.svelte` in the active `ribbon-corner` card: lazy request for document blocks as a card nears the viewport, with a fixed visual budget. Image-first and text-with-small-image layouts are supported; the existing Discovery text is shown until content loads or if it fails. Hidden moderator cards never perform public-content preview reads.
- `dev/localApi.ts`: session bridge permits block moves and removals alongside existing add/update while retaining origin/CSRF restrictions.
- Storybook examples and Playwright regression tests cover editing, ordering, validation, legacy excerpt derivation and unsupported block preservation.

## Compatibility and incomplete work

1. **ShortDescription remains in Graph/SQL and the current create/discovery API.** The client derives a bounded text value for that compatibility field rather than asking the author to maintain it. Removing the field/database projection needs a separately reviewed backend migration, including existing nodes, imports, moderation history and search indexes. Do not remove it abruptly.
2. **The initial multi-block publish is staged, not transactional.** A failure can leave a partially populated, already-created node. A future create-document transaction endpoint would address this. The retry uses the retained pending node ID during the same editor session. Closing or refreshing after partial creation is not yet recoverable.
3. **Feed content loading adds lazy per-visible-card reads.** This avoids delaying Discovery's initial page and limits early requests, but is not the desired large-scale data projection. Evaluate a bounded Content-derived preview projected by the backend or a batched preview endpoint before high-volume use, particularly on sleeping Azure SQL.
4. **Only the active ribbon-corner presentation uses the new media preview.** Other experimental card versions still render a concise text fallback. This is a deliberate first layout slice; keep card variants in sync after design selection.
5. URL-based image authoring is for this initial slice. Production uploads, managed storage and media security are covered by [PTT-167](https://thepublicthinktank.atlassian.net/browse/PTT-167); [PTT-161](https://thepublicthinktank.atlassian.net/browse/PTT-161) covers reproducible development seed images.
6. Poll/video/chart/link-preview references are not interactive in this branch. Unknown content is not deleted. A richer editor screen/mobile bottom-sheet, accessible drag/drop, precise dial-height token, custom preview selection and two-block preview algorithm are **open design work**, not closed decisions.
7. The existing hidden-node moderation repair editor still uses legacy short-description controls. Review its workflow and its moderation audit history before migrating it.
8. The authoring experience has not yet been validated against a real database on all target browsers. Run `npm run check`, `npm run build-storybook`, targeted Playwright tests and API integration round-trips before merge.

## Next acceptance checkpoints

- Create a node containing image first, then Markdown; reload and verify block order/IDs and no duplicated transition block.
- Edit existing Markdown, insert image, move image above it, save/reopen; verify both API persistence and full node display.
- Delete a supported block; verify permission checks and that unknown blocks are still intact.
- On 320px and normal desktop widths, verify preview text/image stays within a bounded space around the voting dial, and the card's Read more action remains unobstructed.
- Confirm moderation hiding cannot leak original media or text to anonymous feed users.
- Test partial write failure, private draft restore, CSRF rejection, URL validation and old browser draft compatibility.
