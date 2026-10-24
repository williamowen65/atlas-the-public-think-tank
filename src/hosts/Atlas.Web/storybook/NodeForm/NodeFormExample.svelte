<script lang="ts">
  import LiveNodeForm from '../../src/lib/components/LiveNodeForm.svelte';
  import { openNodeStory } from '../shared/navigation';
  let {
    parentId,
    parentTitle,
    initialTypeId,
  }: {
    /** Parent node ID for child-node creation; omit for root creation. */
    parentId?: string;
    /** Display title for the parent breadcrumb; parentId is the actual association. */
    parentTitle?: string;
    /** Initial node type selection when creating a draft. */
    initialTypeId?: string;
  } = $props();
</script>

<LiveNodeForm {parentId} {parentTitle} {initialTypeId} onCreated={openNodeStory} />
