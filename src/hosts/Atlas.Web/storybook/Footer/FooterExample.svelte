<script lang="ts">
  import AtlasFooter from '../../src/lib/components/AtlasFooter.svelte';
  import { openStory } from '../shared/navigation';
  let { tagline = 'A place to connect questions, evidence, and ideas.' }: { tagline?: string } =
    $props();
  function navigate(href: string) {
    const destinations: Record<string, string> = {
      '/': 'compositions-feed--playground',
      '/new': 'components-node-form--playground',
      '/about': 'layout-header--playground',
      '/about#how-it-works': 'layout-header--playground',
      '/about#mission': 'layout-header--playground',
    };
    const story = destinations[href];
    if (story) openStory(story);
    else window.location.assign(href);
  }
</script>

<AtlasFooter {tagline} onNavigate={navigate} />
