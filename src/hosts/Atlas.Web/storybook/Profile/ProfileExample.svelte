<script lang="ts">
  import LiveProfilePage from '../../src/lib/components/LiveProfilePage.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
  let { participantId = '' }: { participantId?: string } = $props();
</script>

<LiveProfilePage {participantId} onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />
