<script lang="ts">
  let { count }: { count: number } = $props();
</script>

<svg viewBox="0 0 24 24" width="20" height="20" fill="none"
  stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
  <path d="M21 11.5a8.5 8.5 0 0 1-8.5 8.5H4l1.8-3.6A8.5 8.5 0 1 1 21 11.5Z" />
</svg>
<span class="label">Comments ({count})</span>
<span class="count" aria-hidden="true">{count}</span>

<style>
  svg, .count { display: none; }
  @media (max-width: 560px) {
    svg, .count { display: block; }
    .label { display: none; }
  }
</style>
