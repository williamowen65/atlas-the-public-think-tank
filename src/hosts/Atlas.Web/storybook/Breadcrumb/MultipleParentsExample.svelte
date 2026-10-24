<script lang="ts">
  import Breadcrumb, { type BreadcrumbRoute } from '../../src/lib/components/Breadcrumb.svelte';
  const routes: BreadcrumbRoute[] = [
    {
      id: 'future',
      label: 'Question: A future worth building',
      divergenceIndex: 1,
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
      divergenceIndex: 1,
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
  const separateRoots: BreadcrumbRoute[] = [
    {
      id: 'wildlife',
      label: 'Goal: Protect wildlife habitats',
      divergenceIndex: 2,
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
      divergenceIndex: 2,
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
      divergenceIndex: 2,
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
  let scenario = $state<'shared' | 'different'>('shared');
  let availableRoutes = $derived(scenario === 'shared' ? routes : separateRoots);
  let active = $state(routes[0]);
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
        active = routes[0];
      }}>Shared root</button
    >
    <button
      type="button"
      class:active={scenario === 'different'}
      onclick={() => {
        scenario = 'different';
        active = separateRoots[0];
      }}>Different roots (3 paths)</button
    >
  </div>
  <p class="context">
    Viewing: <strong>How could free internet access be funded and maintained?</strong>
  </p>
  {#key scenario}<Breadcrumb
      items={active.items}
      routes={availableRoutes}
      onRouteChange={(route) => (active = route)}
      onNavigate={(id) =>
        (visited = 'Navigate to: ' + (active.items.find((item) => item.id === id)?.title ?? id))}
    />{/key}
  <p role="status">Current parent route: {active.label}</p>
  <p>{visited}</p>
  <p class="hint">
    The second breadcrumb segment has two possible parents. Click its inline chevron to change that
    segment while keeping the later segments intact.
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
