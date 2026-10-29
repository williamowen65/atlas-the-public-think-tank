<!--
  @component
  Read-only visual inventory of Atlas styles. This story uses the shared application CSS variables
  and real Svelte components rather than storing a second palette, theme, or set of style controls.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  import ActionButton from '../../src/lib/components/ActionButton.svelte';
  import VotingDial from '../../src/lib/components/VotingDial.svelte';
  import NodeCard from '../../src/lib/components/NodeCard.svelte';
  import type { NodeCardData } from '../../src/lib/components/node-card-versions/types';

  const colors = [
    { label: 'Background', token: '--atlas-bg', purpose: 'Page canvas' },
    { label: 'Surface', token: '--atlas-surface', purpose: 'Cards and panels' },
    { label: 'Text', token: '--atlas-text', purpose: 'Primary copy' },
    { label: 'Muted', token: '--atlas-muted', purpose: 'Supporting details' },
    { label: 'Accent', token: '--atlas-accent', purpose: 'Links and actions' },
    { label: 'On accent', token: '--atlas-on-accent', purpose: 'Text on primary actions' },
    { label: 'Border', token: '--atlas-border', purpose: 'Outlines and separators' },
    { label: 'Focus', token: '--atlas-focus', purpose: 'Keyboard focus outline' },
  ] as const;

  const layout = [
    { label: 'Shared spacing', token: '--atlas-space', fallback: '1rem', note: 'Density-aware' },
    { label: 'Card padding', token: '--atlas-card-padding', fallback: '1.25rem', note: 'Component fallback' },
    { label: 'Narrow card padding', token: '--atlas-card-padding-narrow', fallback: '.75rem', note: 'Where supported' },
    { label: 'Dial / text gap', token: '--atlas-dial-text-gap', fallback: '1rem', note: 'Component fallback' },
    { label: 'Corner radius', token: '--atlas-radius', fallback: '.5rem', note: 'Shared global token' },
  ] as const;

  // Each row is measured from the *actual NodeCard instance below*, never an imitation
  // of the component's type hierarchy. Keep selectors aligned with the production markup.
  const typographyTargets = [
    { label: 'Node title', selector: '.node-card > header > h2' },
    { label: 'Author / date', selector: '.node-card > header > .byline' },
    { label: 'Short description', selector: '.node-card .description' },
    { label: 'Contribution invitation', selector: '.node-card .invitation' },
    { label: 'Card details', selector: '.node-card > .card-details' },
  ] as const;

  type TextMetrics = {
    fontSize: string;
    fontWeight: string;
    lineHeight: string;
    marginBottom: string;
  };

  const exampleNode: NodeCardData = {
    id: 'b1000000-0000-4000-8000-000000000040',
    typeName: 'Question',
    title: 'What could make public spaces more welcoming?',
    authorName: 'Atlas example',
    createdAt: '2026-10-01T12:00:00Z',
    description:
      'A reference card displaying the actual title, metadata, voting, reactions and actions together.',
    communities: ['Community ideas'],
    breadcrumb: [],
    related: [],
    requestedTypes: ['Idea', 'Evidence'],
    childTypes: [{ typeId: 'idea', label: 'Ideas (3)', count: 3 }],
    reactions: [{ label: 'Thoughtful', count: 4 }],
    commentCount: 2,
    status: 'Active',
  };

  // The values are read from CSS at runtime, not duplicated in this component.
  let currentTokens = $state<Record<string, string>>({});
  let actualTypography = $state<Record<string, TextMetrics>>({});
  let cardExample: HTMLDivElement;
  let rating = $state<number | null>(5);
  let feedback = $state('These example interactions do not save anything.');

  onMount(() => {
    function refresh() {
      const css = getComputedStyle(document.documentElement);
      currentTokens = {
        ...Object.fromEntries(
          [...colors, ...layout].map(({ token }) => [token, css.getPropertyValue(token).trim()]),
        ),
        'font-family': css.fontFamily,
        'font-size': css.fontSize,
        'line-height': css.lineHeight,
      };

      // The node card's local SCSS intentionally differs from the root text defaults.
      // Read the browser's computed values after all inherited and component styles apply.
      if (cardExample) {
        actualTypography = Object.fromEntries(
          typographyTargets.flatMap(({ label, selector }) => {
            const element = cardExample.querySelector<HTMLElement>(selector);
            if (!element) return [];
            const style = getComputedStyle(element);
            return [
              [
                label,
                {
                  fontSize: style.fontSize,
                  fontWeight: style.fontWeight,
                  lineHeight: style.lineHeight,
                  marginBottom: style.marginBottom,
                },
              ],
            ];
          }),
        );
      }
    }
    refresh();
    const observer = new MutationObserver(refresh);
    observer.observe(document.documentElement, {
      attributes: true,
      attributeFilter: ['style', 'data-theme', 'data-density'],
    });
    // Responsive font sizing on the card can change when its available width changes.
    const resizeObserver = new ResizeObserver(refresh);
    if (cardExample) resizeObserver.observe(cardExample);
    return () => {
      observer.disconnect();
      resizeObserver.disconnect();
    };
  });
</script>

<main class="design-reference">
  <header class="intro">
    <p class="overline">Styles and Layout · Current design reference</p>
    <h1>Atlas design system</h1>
    <p>
      This is a visual inventory of the styles <strong>already used by Atlas</strong>, not a new
      theme builder. Edit the shared CSS or a component's styles, and the same change appears
      here and in the web application.
    </p>
    <p class="fixture-note">
      Compare light/dark themes and comfortable/compact density with the Storybook toolbar.
      The existing Typography Playground is retained for experiments; this overview has no
      design-token editing controls.
    </p>
  </header>

  <section aria-labelledby="colors-title">
    <div class="section-heading">
      <h2 id="colors-title">Color roles</h2>
      <p>These swatches and displayed values read the current CSS variables.</p>
    </div>
    <div class="swatches">
      {#each colors as color (color.token)}
        <div class="swatch-card">
          <div class="swatch" style:background-color={`var(${color.token})`} aria-hidden="true"></div>
          <strong>{color.label}</strong>
          <code>{color.token}</code>
          <span class="value">{currentTokens[color.token] || '—'}</span>
          <small>{color.purpose}</small>
        </div>
      {/each}
    </div>
    <div class="color-context">
      <div class="surface-example">
        <strong>Primary text on surface</strong>
        <p>Muted information, such as an author or date, should remain comfortably readable.</p>
        <a href="#components-title">Accent link on surface</a>
      </div>
      <div class="accent-example">Accent with on-accent text</div>
    </div>
  </section>

  <section aria-labelledby="typography-title">
    <div class="section-heading">
      <h2 id="typography-title">Typography actually used in Atlas</h2>
      <p>
        The shared defaults describe the page's baseline, but node-card text has its own
        component styles. The table measures the
        <a href="#card-title">real node card below</a> instead of showing a second,
        independently styled typography example.
      </p>
    </div>
    <div class="typography-reference">
      <dl class="type-metrics" aria-label="Application baseline typography">
        <div><dt>Root font family</dt><dd>{currentTokens['font-family'] || '—'}</dd></div>
        <div><dt>Root font size</dt><dd>{currentTokens['font-size'] || '—'}</dd></div>
        <div><dt>Root line height</dt><dd>{currentTokens['line-height'] || '—'}</dd></div>
      </dl>
      <div class="type-table-scroll">
        <table class="type-table">
          <caption>Computed typography from the node card in this story</caption>
          <thead>
            <tr>
              <th scope="col">Actual element</th>
              <th scope="col">Font size</th>
              <th scope="col">Weight</th>
              <th scope="col">Line height</th>
              <th scope="col">Bottom margin</th>
            </tr>
          </thead>
          <tbody>
            {#each typographyTargets as target (target.label)}
              <tr>
                <th scope="row">
                  {target.label}
                  <code>{target.selector}</code>
                </th>
                <td>{actualTypography[target.label]?.fontSize || '—'}</td>
                <td>{actualTypography[target.label]?.fontWeight || '—'}</td>
                <td>{actualTypography[target.label]?.lineHeight || '—'}</td>
                <td>{actualTypography[target.label]?.marginBottom || '—'}</td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
      <p class="fixture-note">
        These values are computed by the browser for the card at the current preview width.
        They can change with viewport size or Storybook globals. Card-specific CSS lives in
        <code>NodeCardRibbonCorner.svelte</code>; shared defaults live in <code>styles.scss</code>.
      </p>
    </div>
  </section>

  <section aria-labelledby="layout-title">
    <div class="section-heading">
      <h2 id="layout-title">Spacing, shape and surfaces</h2>
      <p>Some component tokens use fallbacks; an unset value is shown explicitly below.</p>
    </div>
    <div class="two-columns">
      <div class="sample-panel">
        <h3>Layout token inventory</h3>
        {#each layout as item (item.token)}
          <div class="token-row">
            <div class="token-description">
              <strong>{item.label}</strong>
              <code>{item.token}</code>
              <small>{item.note} · {currentTokens[item.token] || `fallback ${item.fallback}`}</small>
            </div>
            <div class="measure" aria-hidden="true">
              <span style:width={`var(${item.token}, ${item.fallback})`}></span>
            </div>
          </div>
        {/each}
      </div>
      <div class="sample-panel">
        <h3>Standard Atlas panel</h3>
        <div class="atlas-panel">
          <strong>Shared panel style</strong>
          <p>This uses the actual surface, border, radius and shared spacing styles.</p>
        </div>
        <p class="fixture-note">
          Not every spacing rule is a global token. Component-specific layouts still live in
          their own Svelte styles.
        </p>
      </div>
    </div>
  </section>

  <section aria-labelledby="components-title">
    <div class="section-heading">
      <h2 id="components-title">Real Atlas controls</h2>
      <p>These are the production components, not pictures or reconstructed examples.</p>
    </div>
    <div class="two-columns">
      <div class="sample-panel">
        <h3>Action button states</h3>
        <div class="action-row">
          <ActionButton
            label="Primary action"
            onActivate={() => (feedback = 'Primary action activated (preview only).')}
          />
          <ActionButton label="Disabled action" disabled />
        </div>
        <p class="fixture-note">Tab to the enabled button to inspect its focus treatment.</p>
      </div>
      <div class="sample-panel">
        <h3>Importance rating</h3>
        <div class="rating-row">
          <VotingDial
            label="Design reference importance"
            value={rating}
            average={7.4}
            totalVotes={21}
            onChange={(next) => (rating = next)}
          />
          <p class="fixture-note">
            {rating === null ? 'Not rated' : `Selected: ${rating} of 10`}
            · Local illustration, no API request.
          </p>
        </div>
      </div>
    </div>
  </section>

  <section aria-labelledby="card-title">
    <div class="section-heading">
      <h2 id="card-title">Putting it together: node card</h2>
      <p>
        Inspect typography, spacing, voting and actions together in the actual ribbon-corner
        presentation. Resize the preview to review responsive behavior.
      </p>
    </div>
    <div class="card-example" bind:this={cardExample}>
      <NodeCard
        node={exampleNode}
        presentation="ribbon-corner"
        value={rating}
        average={7.4}
        totalVotes={21}
        onVote={(next) => (rating = next)}
        onReadMore={() => (feedback = 'Read more activated (preview only).')}
        onComments={() => (feedback = 'Comments activated (preview only).')}
        onChildType={() => (feedback = 'Contribution activated (preview only).')}
      />
    </div>
    <p class="fixture-note" role="status">{feedback}</p>
  </section>

  <footer class="source-note">
    <strong>Where to make changes:</strong>
    <code>src/styles.scss</code> owns shared theme variables.
    Individual <code>src/lib/components/*.svelte</code> files own component-specific CSS.
    <code>storybook/StylesAndLayout/designSettings.ts</code> only applies Storybook experiments.
    Edit production styles, not this overview, to change Atlas.
  </footer>
</main>

<style lang="scss">
  .design-reference {
    max-width: 76rem;
    margin-inline: auto;
    color: var(--atlas-text);
    overflow-wrap: anywhere;
  }
  .intro,
  section {
    margin-bottom: 2.2rem;
  }
  .intro > p {
    max-width: 72ch;
  }
  h1 {
    font-size: clamp(1.7rem, 3vw, 2.5rem);
    line-height: 1.15;
    margin: 0.35rem 0 0.8rem;
  }
  h2 {
    font-size: 1.4rem;
    line-height: 1.25;
    margin: 0 0 0.35rem;
  }
  h3 {
    margin: 0 0 0.8rem;
    font-size: 1.1rem;
  }
  h4 {
    margin: 0 0 0.5rem;
  }
  p {
    margin: 0 0 0.85rem;
  }
  .overline {
    text-transform: uppercase;
    letter-spacing: 0.07em;
    color: var(--atlas-muted);
    font-size: 0.8rem;
  }
  .section-heading {
    margin-bottom: 0.9rem;
  }
  .section-heading p,
  small,
  .value {
    color: var(--atlas-muted);
  }
  .swatches {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 10rem), 1fr));
    gap: var(--atlas-space);
  }
  .swatch-card {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    min-width: 0;
    padding: 0.65rem;
    border: 1px solid var(--atlas-border);
    background: var(--atlas-surface);
    border-radius: var(--atlas-radius);
  }
  .swatch {
    height: 4rem;
    border: 1px solid var(--atlas-border);
    border-radius: calc(var(--atlas-radius) / 2);
    margin-bottom: 0.3rem;
  }
  .swatch-card code,
  .token-row code {
    overflow-wrap: anywhere;
    font-size: 0.78rem;
  }
  .value,
  small {
    font-size: 0.8rem;
  }
  .color-context {
    display: flex;
    flex-wrap: wrap;
    gap: var(--atlas-space);
    margin-top: var(--atlas-space);
  }
  .surface-example,
  .accent-example {
    border-radius: var(--atlas-radius);
    padding: var(--atlas-space);
    flex: 1 1 14rem;
  }
  .surface-example {
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
  }
  .surface-example p {
    color: var(--atlas-muted);
    margin-top: 0.45rem;
  }
  .accent-example {
    background: var(--atlas-accent);
    color: var(--atlas-on-accent);
    align-content: center;
    font-weight: 600;
  }
  .typography-reference,
  .sample-panel {
    background: var(--atlas-surface);
    border: 1px solid var(--atlas-border);
    border-radius: var(--atlas-radius);
    padding: var(--atlas-space);
    min-width: 0;
  }
  .type-metrics {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 10rem), 1fr));
    gap: 0.65rem;
    margin: 0 0 1.3rem;
    padding-bottom: 0.9rem;
    border-bottom: 1px solid var(--atlas-border);
  }
  .type-metrics div {
    min-width: 0;
  }
  .type-metrics dt {
    color: var(--atlas-muted);
    font-size: 0.8rem;
  }
  .type-metrics dd {
    margin: 0.2rem 0 0;
    overflow-wrap: anywhere;
    font-size: 0.9rem;
  }
  .type-table-scroll {
    overflow-x: auto;
    margin-bottom: 0.9rem;
  }
  .type-table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
    font-size: 0.88rem;
  }
  .type-table caption {
    text-align: left;
    padding-bottom: 0.45rem;
    color: var(--atlas-muted);
  }
  .type-table th,
  .type-table td {
    padding: 0.6rem 0.8rem 0.6rem 0;
    border-bottom: 1px solid var(--atlas-border);
    vertical-align: top;
  }
  .type-table td {
    white-space: nowrap;
  }
  .type-table th code {
    display: block;
    margin-top: 0.3rem;
    font-size: 0.75rem;
    font-weight: normal;
    color: var(--atlas-muted);
    overflow-wrap: anywhere;
  }
  .two-columns {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: var(--atlas-space);
  }
  .token-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    padding-block: 0.65rem;
    border-bottom: 1px solid var(--atlas-border);
  }
  .token-description {
    min-width: 0;
    display: grid;
    gap: 0.2rem;
  }
  .measure {
    flex: 0 0 5rem;
    height: 1.25rem;
    display: flex;
    align-items: center;
  }
  .measure span {
    display: block;
    max-width: 100%;
    min-width: 2px;
    height: 1rem;
    background: var(--atlas-accent);
    border-radius: 2px;
  }
  .sample-panel .atlas-panel p {
    margin-top: 0.5rem;
  }
  .action-row {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 0.75rem;
    margin-bottom: 0.75rem;
  }
  .rating-row {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 1rem;
  }
  .rating-row p {
    max-width: 15rem;
  }
  .card-example {
    max-width: 48rem;
  }
  .source-note {
    border-top: 1px solid var(--atlas-border);
    padding-block: 1rem;
    color: var(--atlas-muted);
  }
  .source-note code {
    color: var(--atlas-text);
  }
  @media (max-width: 660px) {
    .two-columns {
      grid-template-columns: minmax(0, 1fr);
    }
    .measure {
      flex-basis: 3.5rem;
    }
  }
</style>
