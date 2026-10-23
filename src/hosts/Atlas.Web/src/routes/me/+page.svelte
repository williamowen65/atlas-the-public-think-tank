<script lang="ts">
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  onMount(() => {
    void goto('/profiles/me', { replaceState: true });
  });
</script>

<p>Opening <a href="/profiles/me">your profile</a>…</p>
