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

<LiveProfilePage {participantId} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />
