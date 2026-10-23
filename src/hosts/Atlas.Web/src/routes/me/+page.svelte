<script lang="ts">
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  onMount(() => { void goto('/profiles', { replaceState: true }); });
</script>
<p>Opening <a href="/profiles">your profile</a>…</p>
