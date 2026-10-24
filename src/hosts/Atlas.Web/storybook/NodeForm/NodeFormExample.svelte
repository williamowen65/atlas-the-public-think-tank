<!--
  @component
  Live root/child creation-form example that opens created nodes in Storybook.
-->
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

<!--
  Layout: The live node form is rendered directly for a root or child context; successful
  creation opens the node story.
-->

<LiveNodeForm {parentId} {parentTitle} {initialTypeId} onCreated={openNodeStory} />
