<!--
  @component
  Legacy profile route that redirects to the current participant’s profile, with a fallback
  link.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  onMount(() => {
    void goto('/profiles/me', { replaceState: true });
  });
</script>

<!--
  Layout: A profile link remains visible while this legacy route redirects to the current
  profile page.
-->

<p>Opening <a href="/profiles/me">your profile</a>…</p>
