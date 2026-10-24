// Fallback for tooling that does not process Svelte component modules.
// Named data types belong in shared .ts modules; this shim only describes the default component.
declare module '*.svelte' {
  import type { Component } from 'svelte';

  const component: Component;
  export default component;
}
