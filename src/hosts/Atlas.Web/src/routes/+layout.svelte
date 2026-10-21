<script lang="ts">
  import '../styles.scss';

  let { children } = $props();
</script>

<nav aria-label="Main navigation">
  <a href="/">Component example</a>
  <a href="/about">About this foundation</a>
</nav>

<main>
  {@render children()}
</main>
