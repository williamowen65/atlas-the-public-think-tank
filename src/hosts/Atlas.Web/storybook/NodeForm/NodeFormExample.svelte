<script lang="ts">
  import LiveNodeForm from '../../src/lib/components/LiveNodeForm.svelte';
  import { openNodeStory } from '../shared/navigation';
  let { parentId, parentTitle, initialTypeId }: { parentId?: string; parentTitle?: string; initialTypeId?: string } = $props();
</script>
<LiveNodeForm {parentId} {parentTitle} {initialTypeId} onCreated={openNodeStory} />
