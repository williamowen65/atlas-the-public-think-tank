<script lang="ts">
  import type { ComponentProps } from 'svelte';
  import AtlasHeader from '../../src/lib/components/AtlasHeader.svelte';
  import { openStory } from '../shared/navigation';
  let {
    active = 'discover',
    authenticated = false,
    displayName = 'Atlas User',
  }: {
    active?: ComponentProps<typeof AtlasHeader>['active'];
    authenticated?: boolean;
    displayName?: string;
  } = $props();
  function navigate(destination: 'discover' | 'create' | 'login' | 'register' | 'profile') {
    const stories = {
      discover: 'compositions-feed--playground',
      create: 'components-node-form--playground',
      login: 'components-sign-in--playground',
      register: 'components-sign-in--register',
      profile: 'compositions-my-profile--playground',
    };
    openStory(stories[destination]);
  }
</script>

<AtlasHeader {active} {authenticated} {displayName} onNavigate={navigate} />
