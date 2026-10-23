<script lang="ts">
  import Breadcrumb, { type BreadcrumbRoute } from '../../src/lib/components/Breadcrumb.svelte';
  const routes: BreadcrumbRoute[] = [
    { id: 'future', label: 'Question: A future worth building', items: [{ id: 'future', title: 'Question: A future worth building' }] },
    { id: 'services', label: 'Goal: Make essential services accessible to everyone', items: [{ id: 'services', title: 'Goal: Make essential services accessible to everyone' }] },
  ];
  let active = $state(routes[0]);
  let visited = $state('Select a breadcrumb to navigate to its parent.');
</script>

<div class="example">
  <p class="context">Idea: <strong>Make internet access free and available everywhere</strong></p>
  <Breadcrumb items={active.items} {routes} onRouteChange={route => active = route} onNavigate={id => visited = 'Navigate to: ' + routes.find(route => route.id === id)?.label} />
  <p role="status">Current context: {active.label}</p>
  <p>{visited}</p>
  <p class="hint">Use the small dropdown at the end of the breadcrumb to switch parent paths without leaving the idea.</p>
</div>

<style>
  .example { max-width: 38rem; min-width: 0; padding: 1rem; }
  .context { margin-bottom: 1rem; }
  .hint { color: var(--atlas-muted); font-size: .9rem; }
</style>
