<script lang="ts">
  import LiveProfilePage from '../../src/lib/components/LiveProfilePage.svelte';
  import { openNodeStory } from '../shared/navigation';
  let { participantId = '' }: { participantId?: string } = $props();
</script>
<LiveProfilePage {participantId} onOpenNode={openNodeStory} />
