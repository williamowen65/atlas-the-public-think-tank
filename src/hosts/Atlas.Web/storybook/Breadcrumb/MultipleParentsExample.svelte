<script lang="ts">
  import Breadcrumb, { type BreadcrumbRoute } from '../../src/lib/components/Breadcrumb.svelte';
  const routes: BreadcrumbRoute[] = [
    { id: 'future', label: 'Question: A future worth building', divergenceIndex: 1, items: [
      { id: 'atlas', title: 'Question: What future should we build?' },
      { id: 'future', title: 'Question: A future worth building' },
      { id: 'internet', title: 'Idea: Make internet access free and available everywhere' },
      { id: 'funding', title: 'Question: How could free internet access be funded and maintained?' },
    ] },
    { id: 'services', label: 'Goal: Make essential services accessible to everyone', divergenceIndex: 1, items: [
      { id: 'atlas', title: 'Question: What future should we build?' },
      { id: 'services', title: 'Goal: Make essential services accessible to everyone' },
      { id: 'internet', title: 'Idea: Make internet access free and available everywhere' },
      { id: 'funding', title: 'Question: How could free internet access be funded and maintained?' },
    ] },
  ];
  let active = $state(routes[0]);
  let visited = $state('Select a breadcrumb to navigate.');
</script>

<div class="example">
  <p class="context">Viewing: <strong>How could free internet access be funded and maintained?</strong></p>
  <Breadcrumb items={active.items} {routes} onRouteChange={route => active = route} onNavigate={id => visited = 'Navigate to: ' + (active.items.find(item => item.id === id)?.title ?? id)} />
  <p role="status">Current parent route: {active.label}</p>
  <p>{visited}</p>
  <p class="hint">The second breadcrumb segment has two possible parents. Click its inline chevron to change that segment while keeping the later segments intact.</p>
</div>

<style>
  .example { max-width: 54rem; min-width: 0; padding: 1rem; }
  .context { margin-bottom: 1rem; }
  .hint { color: var(--atlas-muted); font-size: .9rem; }
</style>
