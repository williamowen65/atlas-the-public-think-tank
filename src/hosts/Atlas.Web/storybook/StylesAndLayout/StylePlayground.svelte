<!--
  @component
  Typography and spacing workbench for exploring shared Storybook design settings and resetting
  code defaults.

  **Props**
  - `fontFamily` (`string`; optional): CSS font-family stack for workbench experiments.
  - `fontSize` (`number`; optional; default `16`): Base font size in pixels.
  - `lineHeight` (`number`; optional; default `1.6`): Unitless text line-height multiplier.
  - `headingScale` (`number`; optional; default `1.5`): Heading size multiplier for the typography
    specimen and shared tokens.
  - `paragraphGap` (`number`; optional; default `16`): Paragraph spacing in pixels.
  - `headingGap` (`number`; optional; default `12`): Heading spacing in pixels.
  - `space` (`number`; optional): Shared spacing override in pixels; zero follows Storybook Density.
  - `cardPadding` (`number`; optional): Standard card padding in pixels.
  - `cardPaddingNarrow` (`number`; optional): Narrow-screen card padding in pixels.
  - `dialTextGap` (`number`; optional): Gap between the voting dial and adjacent text, in pixels.
  - `radius` (`number`; optional): Shared corner radius in pixels.
  - `background` (`string`; optional): Page background CSS color; empty uses the selected theme.
  - `surface` (`string`; optional): Panel surface CSS color; empty uses the selected theme.
  - `text` (`string`; optional): Primary text CSS color; empty uses the selected theme.
  - `muted` (`string`; optional): Secondary text CSS color; empty uses the selected theme.
  - `accent` (`string`; optional): Accent CSS color; empty uses the selected theme.
  - `onAccent` (`string`; optional): Text color over accent surfaces; empty uses the selected theme.
  - `border` (`string`; optional): Border CSS color; empty uses the selected theme.
  - `focus` (`string`; optional): Keyboard-focus CSS color; empty uses the selected theme.
  - `onReset` (`() => void`; optional): Callback: Called when Reset styles to code defaults is
    activated; the parent provides the reset implementation.
-->
<script lang="ts">
  import type { DesignSettings } from './designSettings';
  import ActionButton from '../../src/lib/components/ActionButton.svelte';

  let {
    fontSize = 16,
    lineHeight = 1.6,
    headingScale = 1.5,
    paragraphGap = 16,
    headingGap = 12,
    onReset,
  }: Partial<DesignSettings> & {
    /**
     * Callback: Called when Reset styles to code defaults is activated; the parent provides the
     * reset implementation.
     */
    onReset?: () => void;
  } = $props();
</script>

<!--
  Layout: Reset and scope notes precede a typography specimen; a separate reference section
  explains spacing tokens.
-->

<ActionButton label="Reset styles to code defaults" onActivate={() => onReset?.()} />
<p class="fixture-note">
  These Controls apply across Storybook components and compositions as you switch stories. They do
  not change SPA code defaults. Empty color values follow the Theme toolbar. Shared space at 0
  follows Density; other values override it until reset.
</p>
<section
  class="specimen"
  style:font-size={`${fontSize}px`}
  style:line-height={lineHeight}
  style:--heading-scale={headingScale}
  style:--paragraph-gap={`${paragraphGap}px`}
  style:--heading-gap={`${headingGap}px`}
>
  <h1>Atlas typography</h1>
  <p>Explore the relationship between headings, body text and the space around them.</p>
  <h2>Understanding a complex issue</h2>
  <p>
    A readable page gives ideas room to breathe. Compare text sizes, line height and the distance
    between a heading and its supporting paragraph.
  </p>
  <p>
    This second paragraph makes paragraph spacing visible. <a href="#details">Links</a>,
    <strong>emphasis</strong> and ordinary text share the same foundation.
  </p>
  <h3 id="details">A smaller supporting heading</h3>
  <p>Community knowledge becomes easier to explore when the visual hierarchy stays consistent.</p>
  <ul>
    <li>A short supporting point</li>
    <li>A longer point that wraps onto additional lines on narrow screens.</li>
  </ul>
</section>
<section class="token-notes" aria-label="Atlas spacing conventions">
  <h2>Atlas spacing conventions</h2>
  <p>
    This is both an experiment area and a record of the current defaults. Review changes in the
    node, feed, header, and footer before promoting them into shared SCSS.
  </p>
  <table>
    <thead><tr><th>Token</th><th>Code default</th><th>Used for</th></tr></thead><tbody>
      <tr
        ><td>Card padding</td><td>20px / 1.25rem</td><td>Card side inset and ribbon-to-title gap</td
        ></tr
      >
      <tr><td>Narrow card padding</td><td>12px / .75rem</td><td>Cards at 380px and below</td></tr>
      <tr
        ><td>Dial/text gap</td><td>16px / 1rem</td><td
          >Space outside the dial; its own inner padding adds to the visible gap</td
        ></tr
      >
      <tr
        ><td>Corner action</td><td>12px / .75rem</td><td
          >Text clearance beside Read more; action stays inside the description</td
        ></tr
      >
      <tr><td>Feed gap</td><td>20px / 1.25rem</td><td>Separation between node cards</td></tr>
      <tr
        ><td>Shared space</td><td>16px comfortable / 8px compact</td><td
          >Page, navigation, and panel spacing</td
        ></tr
      >
    </tbody>
  </table>
  <p>
    Color pairs to review together: text/surface, accent/on-accent, muted/surface, and
    focus/background. Experimental colors can reduce contrast; check the Accessibility panel before
    adopting them.
  </p>
</section>

<style lang="scss">
  .token-notes {
    max-width: 65ch;
    margin-top: 2rem;
    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
    }
    th,
    td {
      padding: 0.5rem;
      border: 1px solid var(--atlas-border);
      text-align: left;
    }
  }

  .specimen {
    max-width: 65ch;
    margin-top: var(--atlas-space);
    padding: var(--atlas-space);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }

  h1,
  h2,
  h3 {
    line-height: 1.2;
    margin: 1em 0 var(--heading-gap);
  }

  h1 {
    font-size: calc(1em * var(--heading-scale) * var(--heading-scale));
  }

  h2 {
    font-size: calc(1em * var(--heading-scale));
  }

  h3 {
    font-size: 1.1em;
  }

  p {
    margin: 0 0 var(--paragraph-gap);
  }
</style>
