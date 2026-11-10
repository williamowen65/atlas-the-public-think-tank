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

## Editor UX research prototype (November 9–10: Tiptap mapping workspace)

The editor UX exploration remains **Storybook-only** and does not alter node
creation, node editing, APIs, storage or the production Atlas card. Ordered
content blocks remain the persistence model; Tiptap is an experimental input
surface over that same model.

Open Storybook:
- Content / Editor workbench (experimental) / Compare editors
- Other useful cases: Image first, Phone width, Blank node.
- The standalone Content / Block composer stories remain for testing that
  individual form component. Redundant editor/style preset stories were removed;
  compare the editors and toggle Atlas/editorial text styles in the main workspace.

Run from src/hosts/Atlas.Web after pulling the feature branch:

    npm ci
    npm run storybook

### One shared model, two views

In the Compare editors story, **Existing block forms and Tiptap canvas now
read and modify the same AuthorBlock[] state**. Edit either editor, switch to
the other, and verify the same text, image order, captions and keys. Opening
the inspector beneath the framed node reveals the ordered kind, key/ID and
content summary. No API calls or persistence occur.

The Storybook-only mapping in tiptapBlocks.ts is now intentionally designed
around Atlas block identity rather than treating every paragraph as a brand
new block:
- Each Markdown block becomes one custom atlasText container holding its
  paragraphs, headings, lists, etc., with a stable data-atlas-key.
- Images become atomic, draggable atlasImage nodes with their own key, URL,
  alternative text and caption; a selected image also has move up/down controls.
  Image drops cannot nest inside an atlasText container, so images remain
  separate persisted blocks.
- Unimplemented block kinds appear as protected placeholders, with their
  original data looked up from the shared AuthorBlock[] when converting back.
- Conversion back to AuthorBlock[] preserves an existing block's ID and
  original payload by matching its key; changes to order don't regenerate IDs.
  New text and image blocks get newly generated keys.
- Common rich text (bold, italic, basic links, headings, lists and paragraphs)
  is converted to Atlas Markdown. This is not a fully lossless Markdown codec:
  nested/advanced syntax, mark ordering, paste cases and future embedded types
  still require design and broader tests.

The main editor has the same conceptual **expanded Atlas node frame** as the
full view: optional ancestor breadcrumb, node type, editable title, byline,
community chooser and content area. It deliberately omits a voting dial during
authoring. Edit / Feed / Full node operate on the same content. The feed preview
is still an illustrative bounded content panel, **not a pixel-perfect live
feed card**. The Tiptap document itself no longer has a second nested paper
border; the enclosing Atlas node is the frame.

### Research questions and validation

Compare copy, formatting and image movement in both directions, then inspect
the key list. Try image-first, blank, long multi-paragraph text, and real
320px-ish screens including mobile touch editing.

Tests: tests/editor-workbench.spec.ts (interactive Storybook round trips and
previews) and tests/tiptap-bridge.spec.ts (stable keys/IDs, reordered images,
multi-paragraph Markdown, unsupported block preservation). Run npm run check,
npm run build-storybook and those Playwright tests before considering this
validated; reported test execution status is separate from the implementation.

Remaining: polished image editing, arbitrary caret insertion within long
Markdown groups, full lossless Markdown conversion and paste handling,
preserving unknown blocks against deliberate deletion, real card pixel parity,
true mobile keyboard testing, and production-grade persistence integration.
These are questions to evaluate, **not** commitments or shipping readiness.

Do not wire the Tiptap prototype to live node creation/editing without a
separate design decision, security review and backend round-trip tests.


## November 10 editor feedback: persistent controls and empty block behavior

Screenshots of the Storybook Tiptap editor revealed that the Tiptap BubbleMenu
and FloatingMenu were moving/reshaping unexpectedly: a giant B/I/Link strip,
a plus symbol sometimes filling its menu region and sometimes covering text,
and a full outer editor focus rectangle competing with image selection.
Using the existing floating-menu extensions was convenient, but this control
placement was not suitable for the Atlas node frame.

The **Storybook-only** prototype now uses:

- A **permanent, Atlas-styled formatting toolbar** above the writing area.
  It does not disappear on blur or selection changes.
- A **left gutter reserved outside the text column**. A consistently sized,
  centered, circular + follows the active paragraph or image. Choosing +
  opens a non-overlapping insert-choices row above the document. The menu
  can add a separate Text or Image block or a heading/section break.
- An in-page, accessible link form (HTTP(S) validation) that captures the
  highlighted range before focusing the URL input; no dependence on
  window.prompt inside a Storybook iframe.
- No big editor-wide focus outline. Keyboard focus is indicated unobtrusively
  along the gutter, while a selected image retains its own selection outline.
- A custom Tiptap root document schema that only permits Atlas text, image,
  or protected unsupported blocks at the top level. This avoids the default
  ProseMirror recovery paragraph becoming an anonymous extra Atlas block
  after Backspace.
- The final empty text block remains intentionally as an editable landing
  place (rather than bouncing between blank blocks). Additional empty text
  blocks expose a **Remove empty text block** action; selected images expose
  **Remove image** alongside move and edit actions.
- Continued support for Enter creating a paragraph *within* the current
  Atlas text block. + → Text creates a distinct Atlas block. This distinction
  is part of the ongoing UX research and may be simplified later.

The separate breadcrumb/ancestry UX question was recorded in
[PTT-182](https://thepublicthinktank.atlassian.net/browse/PTT-182):
explore a clearer way to reveal and navigate the full node lineage, potentially
with an affordance on the far left. **No breadcrumb redesign was applied.**

Regression browser checks were added for always-visible formatting controls,
the non-overlapping gutter, links mapping back to Markdown, and stable
Backspace behavior in the last empty text block. These checks need to pass
in CI and be tried manually on real mobile devices. No production editor,
backend, schema, or live Atlas route was touched in this follow-up.


### Small active-block gutter refinement (Nov 10)

For a more compact authoring canvas, the shared Storybook experiment now
keeps **the ProseMirror document at its normal full node-card width**. It does
*not* indent the entire document for a permanent left column. Instead,
only the currently selected top-level Atlas text/image block gets a temporary
left margin to accommodate the adjacent controls. Other text and images remain
full width and left-aligned with the Atlas node content.

The control rail contains **+**, **move section up**, and **move section down**.
The arrows reorder the *whole Atlas content block*, not an individual paragraph
within the Markdown block; their disabled state reflects the first/last position.
Selecting a different block transfers the indentation and rail to that block.
This behavior is the same in Storybook's phone-width simulation, though real
touch-device usability remains to be reviewed. No production components changed.

Browser coverage checks both the rail geometry (not covering the active block)
and full-width alignment of inactive content, and verifies up/down reorder
the same canonical block data visible in the form-based editor.


### November 10: block rail consistency and deletion confirmation

After testing the selected-section rail with real text and images, a bug
remained: Markdown paragraphs could continue underneath the controls while
images indented properly. The previous implementation manually toggled a
CSS class in Tiptap's transaction callback. ProseMirror replaces text-node
DOM while editing, which could remove that manually applied class.

The Storybook editor now uses a **ProseMirror node-decoration plugin** to
apply the active section class as part of its render process. Only the active
top-level Atlas block—text or image—indents; other blocks remain aligned with
the card content. The rail is anchored against the **left edge of the Atlas
node card**, accounting for card padding at desktop and phone widths. The
control rail is deliberately narrow and contains the insert (+), whole-block
up/down arrows, and a small delete (×) control underneath.

Deletion works for selected text, images, and protected unsupported blocks
through the same confirmation. A small alert dialog names the block and
shows a content summary; Cancel leaves the shared authoring state unchanged.
Confirm removes only the target key, and the form-based editor sees the
updated blocks. If it was the last block, a fresh empty text block replaces
it so the document remains editable. An image still has an Edit image metadata
action, but its duplicate move/remove buttons below the document are gone.

Additional Storybook regression checks cover text indentation after typing,
unused block width, the control rail position relative to the *card border*,
cancel/confirm deletion, and preservation of an editable final text block.
These changes remain experimental and do **not** affect production creation,
editing, or persistence.


### November 10: contextual insertion and focus-driven editing controls

The selected-section rail is only visible **while the Tiptap content editor
actually has focus**, not simply whenever ProseMirror remembers its last
selection. Clicking the node header, title/community, other node chrome or
page background hides the rail, closes the insert popup and removes the active
block's indentation. Clicking back into the document restores the controls on
the focused block.

The compact + now expands **from its own position** into a contextual panel.
The panel is a deliberate exception to the non-overlap requirement: it may
temporarily cover writing content while open. Authors can explicitly select
**Before** or **After** the current Atlas block, then add Text, Image or
Section Break. The menu captures the active block's stable key so that
switching insertion direction or opening the image form doesn't change the
target. The Section Break is represented as a new Markdown content block
containing a horizontal rule, not an unsupported top-level document node.

**Heading is a formatting action within a text section**, not an insertable
top-level content block. A Markdown section can have its own heading plus
multiple paragraphs, and the full section moves together. H2 remains on the
persistent toolbar.

When the author *selects text*, a compact contextual formatting toolbar
appears near the selection for Bold, Italic, Link, H2, and List. The
persistent toolbar above the node body stays available too. Both formatting
surfaces apply Tiptap commands to the same underlying selection.

Browser regressions cover before/after ordering around an image,
selection-popup visibility and formatting, no separate Heading block option,
and rail/indent dismissal when clicking other node controls or the page
background. These are research UI interactions in Storybook, **not a
production editor or a replacement for node API persistence**.


### November 10: attach configuration to the actual content (Storybook only)

A long-node review revealed that the existing **Link URL** form appeared far
above the selected text, and the **Image URL / alt text / caption** form was
rendered at the very end of the node, even if the image was inserted in the
middle. These forms need to belong visually to the content they configure.

The experimental workspace now uses **document-relative, anchored forms**:
- Selecting text and choosing Link opens the URL form beside that selection
  inside the writing area. It captures the ProseMirror selection range before
  inputs take focus, and its position is refreshed on transactions and scroll.
- Choosing + → Image (Before/After) creates a **provisional image node at the
  actual insertion location**, with a visible placeholder reserving the
  configuration space. Its image settings form overlays that placeholder,
  rather than appearing at the document footer.
- **Cancel** removes a new provisional image without adding a permanent block.
  **Apply** validates URL and required alt text, then updates that exact image
  by its stable Atlas key. The shared block-form editor sees the result.
- Every existing image renders an **Edit image** affordance on the image
  itself. Editing displays the form near/over that image; saving updates the
  targeted key and caption without depending on where the caret moved.
- Both forms remain associated with the same content when the page scrolls.
  Their behavior is confined to Storybook; no production node editor, API,
  persistence or upload workflow was changed.

The current prototype deliberately uses URL images, not file uploads.
A provisional empty image is temporarily represented in the Storybook
AuthorBlock[] draft until confirmed or cancelled; this is **not** a proposal
to persist incomplete images in production. Future blocks should reuse this
principle: configuration belongs visibly to the relevant content block.

Playwright regression checks cover a link form deep in the document,
provisional insertion before an existing image, Cancel rollback, and editing
image metadata from the image's own control.


### November 10: inline community picker

The Edit view no longer shows the selected community twice. Its editable
select appears **in the byline**, directly after "By You · Draft · Community:",
where the read-only community label appears in Full Node and Feed previews.
The redundant Community row beneath the byline is removed.

**Open question (not decided):** Authors may eventually be able to select
**multiple communities** for a node. The current Storybook workbench uses one
native select only to explore the placement, not to assert single-community
cardinality. If Atlas moves to multi-community membership, evaluate a compact
multiselect, chips or popover inside the same byline location, while preserving
a readable display in the preview views. The actual production model,
permissions, routing and persistence for multiple communities require
separate design work.


### November 10: node title and type placement

The Storybook **Edit** node card now uses the type selector **in the title
where the read-only Idea/Question/Evidence prefix would be**, rather than
showing both a separate Node Type field and a duplicate prefix.

Layout intentionally follows the existing Atlas responsive pattern:
- **Desktop:** the type dropdown is immediately beside the title input.
- **Mobile or Storybook phone-width simulation:** the same selector sits
  **above the title**, not beside it, avoiding an excessively cramped row.
- **Full node / feed previews:** display the type as ordinary title text
  followed by a colon, with no select control.

The community selection is also inline with the byline, with the possibility
of multi-community selection left open for future UX and model discussion.
No production Svelte component, API or database code was changed.


### November 10: deletion confirmation stays with its block

When deleting a text or image section in the Storybook Tiptap editor, the
confirmation should **not** appear below a potentially long node. It now
renders inside the editor's relatively positioned document shell, anchored
to the selected Atlas block's stable key and upper edge. The dialog stays
near that block during scrolling and document layout updates, and its
horizontal position is bounded by the editor width for narrow/mobile
layouts.

While the dialog has keyboard focus, a ProseMirror decoration keeps that
specific block visibly marked as pending deletion. **Cancel** returns to
editing without removing the block; **Delete block** still deletes by the
captured stable key, preserving the existing confirmation requirement.
Playwright checks cover geometry, highlighted pending state and cancel
for both text and image blocks.

Only Storybook prototype, its tests and research documentation were changed.


### November 10: long Markdown sections — pin the rail to the click, not scrolling

A Storybook review tested a *single* Markdown block containing many screens
of paragraphs. The section remains **one Atlas block** (same stable key, one
reorder/delete target), even when authors click and edit far down in it.

**Final research preference:** the control rail (+, up/down, delete) appears
along the left side of the section near the most recently *clicked* caret
position. It is **pinned to that document position**: scrolling does not
move or clamp the rail into the viewport, nor does the rail chase the
reader. It naturally scrolls offscreen with that location and reappears if
the user scrolls back. Clicking a different paragraph within that same
section reanchors the rail there. Typing in place does not repeatedly
reposition it. Clicking out of the editor hides the rail and focus styling.

A subtle selected-section tint and left-edge marker distinguish the entire
long block from surrounding blocks. For more even spacing, the marker is
drawn **halfway through the gap between the rail and the text**. The tinted
background extends slightly into the available gap and around the content
using a decorative pseudoelement; **the text's horizontal position does not
move any farther**. Inactive blocks keep their normal full-width alignment.

The Storybook Playwright long-section scenario creates 80 paragraphs in
**one** Markdown block and verifies the rail anchors at the clicked line,
retains its document position while scrolling, moves only after another click,
has balanced rail/marker/text spacing, and disappears on editor blur.

Only Storybook, its tests, and this research note changed. The production
node editor and persistence remain untouched.
