# Atlas's component and style workbench

This workbench is for trying UI states before assembling the website. The three top-level sidebar paths are:

| Area | Purpose | Current examples |
| --- | --- | --- |
| Styles and Layout | Isolate shared colors, typography, spacing, surfaces and responsive grids | Typography / Playground |
| Components | One story per reusable component; variations through Controls | ActionButton and VotingDial |
| Compositions | Several production components or content regions working together | Node voting preview and independent voting comparison |

Each metadata `title` sets a slash-separated sidebar path. Each group currently exports only one Playground story to keep navigation simple. Use Controls for compact, disabled, label and selection variations instead of separate sidebar entries. An example wrapper holds local demonstration state but imports the production component; it does not duplicate the component implementation. There is one ActionButton group, with no separate Foundation/Examples split.

## Global styles versus individual controls

`src/styles.css` is the single shared stylesheet imported by the app layout and `.storybook/preview.ts`. Its CSS custom properties define the starter visual tokens: background, surface, text, muted text, accent/on-accent, border, focus, spacing and radius. This is an initial style foundation, not a commitment to Bootstrap, Tailwind or another CSS framework.

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

This first interaction prototype uses visible increase/decrease buttons and a focusable spinbutton: arrows change the value, Home selects 0 and End selects 10. Pointer/touch users can activate the buttons. The first adjustment from unrated starts at 5. It deliberately does not yet reproduce the legacy scrolling reel, inertial gesture, color animation, debounced auto-save, confirmation modal or vote-detail popup. Those are reviewable follow-ups after settling the interaction. The count is informational, not a fake working link. The new people icon is a small accessible-decorative SVG, not the old Inkscape export with duplicate IDs.

Review keyboard and screen-reader usability, pointer target sizes, narrow layouts, light/dark contrast and the distinction between community average and personal selection. Automated accessibility checks are partial, not a full accessibility certification.

## Testing now and next

Current browser coverage exercises standard/compact selection, zero versus unrated, disabled controls, multiple instances, shared theme/density application, stylesheet duplication and basic accessibility. Existing ActionButton and actual SvelteKit route tests remain.

Next component slices can add header states, a complete NodeCard and a seeded feed/list composition. These are directions, not implemented production pages. Redux (PTT-148), live API/session integration (PTT-147), durable voting, production theming and final CSS-framework selection remain separate.
