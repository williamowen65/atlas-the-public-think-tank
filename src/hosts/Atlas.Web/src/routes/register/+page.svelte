<!--
  @component
  Account registration route using SessionControl in initial registration mode.

  **Props**
  No explicit component props. SvelteKit selects this route; URL parameters are read by the route itself.
-->
<script lang="ts">
  import SessionControl from '$lib/components/SessionControl.svelte';
</script>

<!--
  Layout: Page heading followed by the expanded shared session control, initially in
  registration mode.
-->

<svelte:head><title>Create an account · Atlas</title></svelte:head>
<section class="auth-page">
<h1>Create an account</h1>
<SessionControl expanded startRegister />
</section>
