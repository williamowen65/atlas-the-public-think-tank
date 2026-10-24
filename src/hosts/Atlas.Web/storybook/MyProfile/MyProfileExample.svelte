<!--
  @component
  Current-participant live profile example with Storybook navigation.
-->
<script lang="ts">
  import LiveMyProfile from '../../src/lib/components/LiveMyProfile.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
</script>

<!--
  Layout: The current-participant profile composition is rendered directly with Storybook
  navigation callbacks.
-->

<LiveMyProfile onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />
