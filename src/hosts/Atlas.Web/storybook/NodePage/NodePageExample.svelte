<!--
  @component
  Live full-node workbench with an initial discussion-tab option and Storybook navigation.

  **Props**
  - `nodeId` (`string`; optional; default `''`): ID of the node this component reads, displays, or
    modifies.
  - `initialTypeId` (`string`; optional; default `''`): Discussion tab to select when the node story
    opens: a contribution type ID or comments. This is not a new-node type selection.
-->
<script lang="ts">
  import LiveNodePage from '../../src/lib/components/LiveNodePage.svelte';
  import { openStory, openNodeStory, openProfileStory } from '../shared/navigation';
  let {
    nodeId = '',
    initialTypeId = '',
  }: {
    /** ID of the node this component reads, displays, or modifies. */
    nodeId?: string;
    /**
     * Discussion tab to select when the node story opens: a contribution type ID or comments. This
     * is not a new-node type selection.
     */
    initialTypeId?: string;
  } = $props();
</script>

<!--
  Layout: A workbench back button precedes the live node page; callbacks keep nested navigation
  within stories.
-->

<button onclick={() => openStory('compositions-feed--playground')}>Back to feed</button>
<LiveNodePage
  {nodeId}
  {initialTypeId}
  onOpenNode={openNodeStory}
  onOpenProfile={openProfileStory}
/>
