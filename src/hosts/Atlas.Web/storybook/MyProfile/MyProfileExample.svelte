<script lang="ts">
  import LiveMyProfile from '../../src/lib/components/LiveMyProfile.svelte';
  import { openNodeStory, openProfileStory } from '../shared/navigation';
</script>

<LiveMyProfile onOpenNode={openNodeStory} onOpenProfile={openProfileStory} />
