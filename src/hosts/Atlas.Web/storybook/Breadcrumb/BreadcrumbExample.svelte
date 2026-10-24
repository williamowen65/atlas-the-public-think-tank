<script lang="ts">
  import Breadcrumb from '../../src/lib/components/Breadcrumb.svelte';
  let {
    depth = 8,
  }: {
    /** Number of generated ancestor labels in the Storybook breadcrumb demonstration. */
    depth?: number;
  } = $props();
  let selected = $state('No ancestor selected');
  const titles = [
    'What future is worth building?',
    'Our natural world',
    'Healthy oceans',
    'Southern Resident orcas',
    'Actions to help orcas',
    'Protect the food supply',
    'Restore salmon habitat',
    'Local restoration projects',
  ];
  const route = $derived(titles.slice(0, depth).map((title, i) => ({ id: String(i), title })));
  const items = $derived(route.slice(-3));
  async function loadOlder(oldestId?: string) {
    await new Promise((resolve) => setTimeout(resolve, 250));
    const end = Number(oldestId);
    return { items: route.slice(Math.max(0, end - 3), end), hasMore: end > 3 };
  }
</script>

<!-- Layout: The breadcrumb sits above a status message reporting the selected ancestor. -->

<div class="example">
  <Breadcrumb
    {items}
    hasMore={depth > 3}
    {loadOlder}
    onNavigate={(id) => (selected = titles[Number(id)])}
  />
  <p role="status">{selected}</p>
</div>

<style lang="scss">
  .example {
    max-width: 36rem;
    min-width: 0;
  }
</style>
