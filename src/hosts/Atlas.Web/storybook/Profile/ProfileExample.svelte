<!--
  @component
  Live public participant-profile example with Storybook navigation callbacks.
-->
<script lang="ts">
  import LiveProfilePage from '../../src/lib/components/LiveProfilePage.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
  let {
    participantId = '',
  }: {
    /** Participant whose public profile and authored contributions are displayed. */
    participantId?: string;
  } = $props();
</script>

<!--
  Layout: The live participant profile is rendered directly with Storybook node/profile
  navigation.
-->

<LiveProfilePage {participantId} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />
