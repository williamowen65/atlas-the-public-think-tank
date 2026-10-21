# Atlas's component and style workbench

This workbench is for trying UI states before assembling the website. The three top-level sidebar paths are:

| Area | Purpose | Current examples |
| --- | --- | --- |
| Styles and Layout | Isolate shared colors, typography, spacing, surfaces and responsive grids | Typography / Playground |
| Components | One story per reusable component; variations through Controls | ActionButton and VotingDial |
| Compositions | Several production components or content regions working together | Node voting preview and independent voting comparison |

Each metadata `title` sets a slash-separated sidebar path. Each group currently exports only one Playground story to keep navigation simple. Use Controls for compact, disabled, label and selection variations instead of separate sidebar entries. An example wrapper holds local demonstration state but imports the production component; it does not duplicate the component implementation. There is one ActionButton group, with no separate Foundation/Examples split.

## Global styles versus individual controls

`src/styles.scss` is the single shared SCSS stylesheet imported by the app layout and `.storybook/preview.ts`. Its CSS custom properties define the starter visual tokens: background, surface, text, muted text, accent/on-accent, border, focus, spacing and radius. Component and example styles use scoped `<style lang="scss">` blocks, readable declarations and nested state/layout rules. The installed Sass compiler runs automatically through Vite/Svelte preprocessing in development, Storybook, builds and CI. See [the styling workflow](../README.md#styling-with-sass) for file ownership, imports and why styles are shared across instances. This is an initial style foundation, not a commitment to Bootstrap, Tailwind or another CSS framework.

The Storybook **Theme** toolbar chooses light/dark tokens and **Density** chooses comfortable/compact spacing. These globals apply across stories and remain selected when switching stories. The preview decorator puts the selected values on the preview document; CSS resolves the shared tokens. Global styles are therefore deliberately separate from each component's arguments. The app currently uses default light/comfortable tokens; this does not implement an application theme preference screen or persistence.

The typography playground's Controls change body font size, line height, heading scale, paragraph spacing and heading-to-text spacing. The reset button restores the defaults defined in the story and updates Controls as well. Refresh is not a reset: Storybook can retain edited arguments in the URL/current story state. For any other story, use the Controls panel's reset action to restore its args; global Theme/Density are separate toolbar values and should be returned to light/comfortable there. These edits do not modify source files. Use those together with viewport resizing and keyboard focus to inspect wrapping and rhythm. Component Controls change only that component fixture's inputs. Theme and density live in the toolbar rather than being repeated as props on every component.

A future header, NodeCard and feed can consume the same tokens and be reviewed separately or in compositions. Compositions should use fixture data/callbacks at first; they need not become real routes. Real route navigation remains an app test, because Storybook's navigation mocks do not prove deployed URL behavior.

## Legacy voting dial inspection

Inspected the legacy wiki's [overview diagram and animated dial](https://github.com/williamowen65/atlas-the-public-think-tank/wiki/1.0.-Overview) (files `imgs/voting-dial.png` and `imgs/voting-dial.gif` in the wiki repository), plus:

- [Razor dial](../../../../legacy/www.thepublicthinktank.com/Views/Shared/Components/_voteDial.cshtml)
- [Average partial](../../../../legacy/www.thepublicthinktank.com/Views/Shared/Components/_voteAverage.cshtml)
- [Vote count partial](../../../../legacy/www.thepublicthinktank.com/Views/Shared/Components/_voteCount.cshtml)
- [Dial CSS](../../../../legacy/www.thepublicthinktank.com/wwwroot/css/vote-dial.css)
- [Scroll, radio and confirmation behavior](../../../../legacy/www.thepublicthinktank.com/wwwroot/js/Content/initialize-card.js)

The diagram places the average in a box above a vertical selector showing neighboring values; a people icon and total sit beneath it. The GIF shows scrolling, changing selection, color changes and aggregate updates. The old implementation uses hidden radios, positioned labels, IntersectionObserver/scroll synchronization and debounced confirmation.

The repeated stylesheet issue is concrete: `_voteDial.cshtml` renders a `<style>` block with selectors generated from the content ID for each instance; `_voteAverage.cshtml` embeds another style block. The new component instead has a single scoped CSS definition compiled into shared assets, no content-ID-generated selectors and no runtime-created `<style>` blocks per instance. Runtime layout custom properties in the style playground are individual values, not duplicated stylesheet rules. The browser tests mount multiple dials and check that no style blocks appear inside them.

## New voting prototypes and their limits

`VotingDial.svelte` preserves the average / selection / people-and-total hierarchy. Standard shows neighboring numbers vertically. Compact places a reduced control alongside its summary for tighter cards. Unrated (`null`) is distinct from the valid rating zero. Use the disabled and compact Controls, and edit the label to explore comment-layout wording. Set initialValue to null for unrated or zero for a zero rating.

The caller owns `value`; the component reports `onChange(value)` and never fetches or saves. The wrapper displays `Preview selection` and labels average/total as unchanged fixtures. This prevents a local UI experiment from pretending an API vote was saved. The comment story explores space and wording only; it does not add numerical comment ratings to the current domain or replace comment reactions.

The visible arrows, Average/votes labels and Unrated/selection caption are removed. The enlarged people icon contains the centered count. Average and count retain accessible descriptions. Unrated (`null`) shows a neutral dial centered on 5; choosing any value, including zero, changes the selection window to the accent color. Screen readers continue to receive the unrated state explicitly. The existing light/dark tokens supply both color states.

The focusable spinbutton supports arrow keys, Home (0) and End (10). The standard reel displays adjacent values; compact shows only the selected square. Both support wheel/trackpad input and vertical mouse/pen dragging. The standard center lens follows 30% of the pull distance, capped at 16px, during pointer/touch gestures, then eases back to frame the selected row on mouse release/cancel or after native touch scrolling settles; reduced-motion removes that animation. The compact lens stays fixed while the numbers move beneath it. Touch uses native overflow scrolling with momentum and CSS scroll snap. Selection follows the row crossing the center threshold, calculated from scroll position and rendered row height; no IntersectionObserver or generated per-instance styles are needed. Caller updates from scrolling do not recenter the reel, preserving native momentum. External value/layout changes synchronize its position. Disabled controls reject all input. Programmatic snapping respects reduced-motion preferences.

This remains a local controlled preview: there is no auto-save, confirmation, vote-detail popup or accessibility-preference screen. A future alternate input can use the same value/onChange contract. The count is informational, not a working link.
Review keyboard and screen-reader usability, pointer target sizes, narrow layouts, light/dark contrast and the distinction between community average and personal selection. Automated accessibility checks are partial, not a full accessibility certification.

## Testing now and next

Current browser coverage exercises wheel, mouse dragging, native touch scrolling/snap and keyboard selection in standard/compact layouts, count placement and removed labels, rated/unrated color changes, zero versus unrated, disabled controls, multiple instances, shared theme/density application, stylesheet duplication and basic accessibility. Existing ActionButton and actual SvelteKit route tests remain.

Next component slices can add header states, a complete NodeCard and a seeded feed/list composition. These are directions, not implemented production pages. Redux (PTT-148), live API/session integration (PTT-147), durable voting, production theming and final CSS-framework selection remain separate.


## Node card preview

`src/lib/components/NodeCard.svelte` is a reusable presentation component with its own scoped SCSS. It carries forward the legacy card's left-floating dial and wrapping description, author/date, contextual path, expand/collapse and footer. It presents current Atlas node types, communities, requested child types, reactions, connected nodes and comment counts rather than assuming every node is an Issue or Solution. Only the normal dial is used; compact is unchanged.

Open **Compositions / Node voting preview / Playground**, then choose **selectedNode** in Controls: question, issue or idea. Connected-node buttons update the same argument and its Control. Read more expands the description. Each fixture remembers its own local rating until the story remounts; selecting a new node resets its expanded view. Comments opens an illustrative local discussion, including an empty state. No votes or comments are submitted, and average/count fixtures stay unchanged.

Fixture IDs, titles and first description paragraphs follow `DemoData.SeedNode` / `SeedBlock`; additional text, author display names, associations, discussion and aggregates are illustrative. They are not a database snapshot. The legacy reference is `_issue-card.cshtml`; the current Discovery projection is `Atlas.Discovery/DiscoveryCard.cs`.

Storybook can run real event handlers and controlled state. The source Control now also offers **local-api**: see [local API setup and the combined launcher](../README.md#preview-real-local-nodes). This reads the actual public Discovery projection through a development-only anonymous proxy. The reusable adapter supplies actual title, content, author, type, timestamp, community/context text, reactions/child summaries, comments count and vote aggregates. The browser selector, search, paging and refresh operate on database results. Ratings remain local and no authentication/session is forwarded. Static builds and CI continue using fixtures or intercepted responses. Full blocks, live discussions/links and signed-in writes remain follow-ups.


## Uncast vote feedback

The live API card is anonymous: selecting a value is an attempt, not a saved rating. After 650ms without another selection change it returns to neutral (`value = null`, centered on 5), restores the unrated color and explains that sign-in is needed. Switching nodes also clears anonymous attempts.

The dial and fixture-card Playground expose `voteOutcome`: `accepted` retains a simulated successful selection, `anonymous` resets with sign-in feedback, and `failed` resets with retry feedback. These scenarios never write to the API. The live source always uses anonymous behavior regardless of this fixture Control. The shared story helper cancels pending callbacks when unmounted. The production dial remains caller-controlled; a future authenticated adapter must handle server rejection, network failure or canceled submission by returning the dial to null. This slice demonstrates those outcomes rather than claiming authenticated casting is implemented.
