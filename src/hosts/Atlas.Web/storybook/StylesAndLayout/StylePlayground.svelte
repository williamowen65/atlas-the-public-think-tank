<script lang="ts">
  import ActionButton from '../../src/lib/components/ActionButton.svelte';

  let { fontSize = 16, lineHeight = 1.6, headingScale = 1.5, paragraphGap = 16, headingGap = 12, onReset }: {
    fontSize?: number; lineHeight?: number; headingScale?: number;
    paragraphGap?: number; headingGap?: number; onReset?: () => void;
  } = $props();
</script>

<ActionButton label="Reset typography to code defaults" onActivate={() => onReset?.()} />
<p class="fixture-note">Typography Controls affect only this specimen. Theme and Density in the toolbar affect all stories.</p>
<section class="specimen" style:font-size={`${fontSize}px`} style:line-height={lineHeight}
  style:--heading-scale={headingScale} style:--paragraph-gap={`${paragraphGap}px`} style:--heading-gap={`${headingGap}px`}>
  <h1>Atlas typography</h1>
  <p>Explore the relationship between headings, body text and the space around them.</p>
  <h2>Understanding a complex issue</h2>
  <p>A readable page gives ideas room to breathe. Compare text sizes, line height and the distance between a heading and its supporting paragraph.</p>
  <p>This second paragraph makes paragraph spacing visible. <a href="#details">Links</a>, <strong>emphasis</strong> and ordinary text share the same foundation.</p>
  <h3 id="details">A smaller supporting heading</h3>
  <p>Community knowledge becomes easier to explore when the visual hierarchy stays consistent.</p>
  <ul><li>A short supporting point</li><li>A longer point that wraps onto additional lines on narrow screens.</li></ul>
</section>

<style lang="scss">
  .specimen {
    max-width: 65ch;
    margin-top: var(--atlas-space);
    padding: var(--atlas-space);
    background: var(--atlas-surface);
    color: var(--atlas-text);
  }

  h1, h2, h3 {
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
