<!--
  @component
  Live root/child creation-form example that opens created nodes in Storybook.

  **Props**
  - `parentId` (`string`; optional): Parent node ID for child-node creation; omit for root creation.
  - `parentTitle` (`string`; optional): Display title for the parent breadcrumb; parentId is the
    actual association.
  - `initialTypeId` (`string`; optional): Initial node type selection when creating a draft.
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
