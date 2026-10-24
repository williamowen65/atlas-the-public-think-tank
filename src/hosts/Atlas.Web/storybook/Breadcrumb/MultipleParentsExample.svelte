<!--
  @component
  Alternative-parent breadcrumb scenarios demonstrating shared and separate ancestry routes.

  **Props**
  No explicit component props; this example supplies its own composition.
-->
<script lang="ts">
  import { onMount } from 'svelte';
  // MSW (Mock Service Worker) simulates HTTP responses in the browser for this story.
  // msw/browser starts interception; msw/http in parentPathHandlers defines responses.
  // Both imports come from the one msw package: run npm install after pulling new dependencies.
  import { setupWorker } from 'msw/browser';
  import Breadcrumb from '../../src/lib/components/Breadcrumb.svelte';
  import { parentPathHandlers, type ExampleRoute } from './parentPathHandlers';

  const routes: ExampleRoute[] = [
    {
      id: 'future',
      label: 'Question: A future worth building',
      items: [
        { id: 'atlas', title: 'Question: What future should we build?' },
        { id: 'future', title: 'Question: A future worth building' },
        { id: 'internet', title: 'Idea: Make internet access free and available everywhere' },
        {
          id: 'funding',
          title: 'Question: How could free internet access be funded and maintained?',
        },
      ],
    },
    {
      id: 'services',
      label: 'Goal: Make essential services accessible to everyone',
      items: [
        { id: 'atlas', title: 'Question: What future should we build?' },
        { id: 'services', title: 'Goal: Make essential services accessible to everyone' },
        { id: 'internet', title: 'Idea: Make internet access free and available everywhere' },
        {
          id: 'funding',
          title: 'Question: How could free internet access be funded and maintained?',
        },
      ],
    },
  ];
  const separateRoots: ExampleRoute[] = [
    {
      id: 'wildlife',
      label: 'Goal: Protect wildlife habitats',
      items: [
        { id: 'nature', title: 'Question: How can nature thrive alongside people?' },
        { id: 'ecosystems', title: 'Issue: Fragmented ecosystems' },
        { id: 'habitat', title: 'Goal: Protect wildlife habitats' },
        { id: 'corridors', title: 'Idea: Connect urban wildlife corridors' },
        { id: 'crossings', title: 'Question: Where should new crossings be built?' },
      ],
    },
    {
      id: 'transport',
      label: 'Goal: Make streets safer for everyone',
      items: [
        { id: 'cities', title: 'Question: What makes a great city?' },
        { id: 'mobility', title: 'Issue: Dangerous road crossings' },
        { id: 'streets', title: 'Goal: Make streets safer for everyone' },
        { id: 'corridors', title: 'Idea: Connect urban wildlife corridors' },
        { id: 'crossings', title: 'Question: Where should new crossings be built?' },
      ],
    },
    {
      id: 'resilience',
      label: 'Goal: Strengthen climate resilience',
      items: [
        { id: 'climate', title: 'Question: How should we adapt to a changing climate?' },
        { id: 'floods', title: 'Issue: More frequent flooding' },
        { id: 'green', title: 'Idea: Restore green infrastructure' },
        { id: 'resilience', title: 'Goal: Strengthen climate resilience' },
        { id: 'corridors', title: 'Idea: Connect urban wildlife corridors' },
        { id: 'crossings', title: 'Question: Where should new crossings be built?' },
      ],
    },
  ];
  /**
   * Selected fixture scenario: shared ancestry or separate roots.
   */
  let scenario = $state<'shared' | 'different'>('shared');
  /**
   * Fixture parent routes matching the current scenario.
   */
  const availableRoutes = $derived(scenario === 'shared' ? routes : separateRoots);
  /**
   * Label of the fixture path most recently selected through the mocked ancestor endpoint.
   */
  let selectedLabel = $state(routes[0].label);
  /** Mount the real component only after the story’s HTTP mocks are ready. */
  let ready = $state(false);
  /** Worker startup failures should be visible rather than leaving an empty example. */
  let error = $state('');

  // Keep the real Breadcrumb API-driven: mock its network boundary in Storybook,
  // rather than adding fixture-only props or a second path-switching implementation.
  // This isolated example needs no running Atlas.Api; other live stories still do.
  onMount(() => {
    // Startup is asynchronous; do not mount the example if it was removed meanwhile.
    let alive = true;
    const worker = setupWorker(
      ...parentPathHandlers(
        () => availableRoutes,
        (label) => {
          selectedLabel = label;
        },
      ),
    );
    void worker
      .start({
        // Generated MSW worker served by Storybook's staticDirs (.storybook/public).
        // It is separate from the Atlas application and is not shipped in the SPA build.
        serviceWorker: { url: './mockServiceWorker.js' },
        // Only our dedicated mock prefix has handlers. Let unrelated traffic continue normally.
        onUnhandledFrame: 'bypass',
        // Suppress routine mock request logs, not startup failures shown below.
        quiet: true,
      })
      .then(() => {
        // Wait for interception before Breadcrumb makes its initial parent requests.
        if (alive) ready = true;
        else worker.stop();
      })
      .catch((cause) => {
        if (alive)
          error = cause instanceof Error ? cause.message : 'Could not start example requests.';
      });
    return () => {
      alive = false;
      // Deactivate this story’s mocking when its component is removed.
      // stop does not uninstall the browser’s registered worker script.
      worker.stop();
    };
  });
  /**
   * Status text showing the simulated navigation destination.
   */
  let visited = $state('Select a breadcrumb to navigate.');
</script>

<!--
  Layout: Scenario switches precede the breadcrumb; route status and explanatory text follow the
  chosen ancestry path.
-->

<div class="example">
  <div class="scenario">
    <button
      type="button"
      class:active={scenario === 'shared'}
      onclick={() => {
        scenario = 'shared';
        selectedLabel = routes[0].label;
      }}>Shared root</button
    >
    <button
      type="button"
      class:active={scenario === 'different'}
      onclick={() => {
        scenario = 'different';
        selectedLabel = separateRoots[0].label;
      }}>Different roots (3 paths)</button
    >
  </div>
  <p class="context">
    Viewing: <strong>How could free internet access be funded and maintained?</strong>
  </p>
  {#if ready}
    {#key scenario}
      <Breadcrumb
        items={availableRoutes[0].items}
        originId="example-origin"
        apiBase="/storybook-breadcrumb-api"
        onNavigate={(id) => {
          const item = availableRoutes
            .flatMap((route) => route.items)
            .find((item) => item.id === id);
          visited = 'Navigate to: ' + (item?.title ?? id);
        }}
      />
    {/key}
  {:else if error}
    <p role="alert">{error}</p>
  {:else}
    <p role="status">Preparing parent-path example…</p>
  {/if}
  <p role="status">Current parent route: {selectedLabel}</p>
  <p>{visited}</p>
  <p class="hint">
    Click an inline chevron to choose another parent. Storybook mocks the same HTTP endpoints used
    by the application; path switching runs inside the real Breadcrumb component.
  </p>
</div>

<style>
  .example {
    max-width: 54rem;
    min-width: 0;
    padding: 1rem;
  }
  .context {
    margin-bottom: 1rem;
  }
  .scenario {
    display: flex;
    gap: 0.5rem;
    margin-bottom: 1rem;
  }
  .scenario button {
    padding: 0.45rem 0.8rem;
    border: 1px solid var(--atlas-border);
    border-radius: 0.35rem;
    cursor: pointer;
  }
  .scenario button.active {
    font-weight: 700;
    outline: 2px solid var(--atlas-text);
  }
  .hint {
    color: var(--atlas-muted);
    font-size: 0.9rem;
  }
</style>
