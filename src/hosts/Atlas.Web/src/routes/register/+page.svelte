<!--
  @component
  Account registration route using SessionControl in initial registration mode.
-->
<script lang="ts">
  import SessionControl from '$lib/components/SessionControl.svelte';
</script>

<!--
  Layout: Page heading followed by the expanded shared session control, initially in
  registration mode.
-->

<svelte:head><title>Create an account · Atlas</title></svelte:head>
<h1>Create an account</h1>
<SessionControl expanded startRegister />
