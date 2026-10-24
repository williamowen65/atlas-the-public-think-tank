<!--
  @component
  Shared header example with story-controlled navigation and account presentation.

  **Props**
  - `active` (`ComponentProps<typeof AtlasHeader>['active']`; optional; default `'discover'`):
    Navigation item highlighted in the isolated header; accepts the same destinations as
    AtlasHeader.active.
  - `authenticated` (`boolean`; optional; default `false`): Selects the signed-in header menu versus
    login/register links; does not authenticate by itself.
  - `displayName` (`string`; optional; default `'Atlas User'`): Name used to derive the
    account-avatar initial.
-->
<script lang="ts">
  import type { ComponentProps } from 'svelte';
  import AtlasHeader from '../../src/lib/components/AtlasHeader.svelte';
  import { openStory } from '../shared/navigation';
  import { storyLinks } from '../shared/linkNavigation';
  let {
    active = 'discover',
    authenticated = false,
    displayName = 'Atlas User',
  }: {
    /**
     * Navigation item highlighted in the isolated header; accepts the same destinations as
     * AtlasHeader.active.
     */
    active?: ComponentProps<typeof AtlasHeader>['active'];
    /**
     * Selects the signed-in header menu versus login/register links; does not authenticate by
     * itself.
     */
    authenticated?: boolean;
    /** Name used to derive the account-avatar initial. */
    displayName?: string;
  } = $props();
  /** Adapt app URLs only in this workbench wrapper; AtlasHeader has no story navigation props. */
  function navigate(url: URL): boolean {
    const stories: Record<string, string> = {
      '/': 'compositions-feed--playground',
      '/new': 'components-node-form--playground',
      '/login': 'components-sign-in--playground',
      '/register': 'components-sign-in--register',
      '/profiles/me': 'compositions-my-profile--playground',
    };
    const story = stories[url.pathname];
    if (!story) return false;
    openStory(story);
    return true;
  }
</script>

<!--
  Layout: The shared header is rendered directly with story-controlled account/navigation state.
-->

<div use:storyLinks={navigate}>
  <AtlasHeader {active} {authenticated} {displayName} />
</div>
