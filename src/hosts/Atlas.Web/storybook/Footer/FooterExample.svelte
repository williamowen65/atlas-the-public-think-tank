<!--
  @component
  Shared footer example with workbench-aware navigation.

  **Props**
  - `tagline` (`string`; optional; default `'A place to connect questions, evidence, and ideas.'`):
    Supporting text in the footer.
-->
<script lang="ts">
  import AtlasFooter from '../../src/lib/components/AtlasFooter.svelte';
  import { openStory } from '../shared/navigation';
  import { storyLinks } from '../shared/linkNavigation';
  let {
    tagline = 'A place to connect questions, evidence, and ideas.',
  }: {
    /** Supporting text in the footer. */
    tagline?: string;
  } = $props();
  function navigate(url: URL): boolean {
    const destinations: Record<string, string> = {
      '/': 'compositions-feed--playground',
      '/new': 'components-node-form--playground',
      '/about': 'layout-header--playground',
      '/about#how-it-works': 'layout-header--playground',
      '/about#mission': 'layout-header--playground',
    };
    const story = destinations[url.pathname + url.hash];
    if (!story) return false;
    openStory(story);
    return true;
  }
</script>

<!--
  Layout: The shared footer is rendered directly; its navigation callback adapts internal links
  for the workbench.
-->

<div use:storyLinks={navigate}>
  <AtlasFooter {tagline} />
</div>
