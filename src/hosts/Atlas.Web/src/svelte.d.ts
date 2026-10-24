// This fallback declares the default component export. Named exports could be
// listed here too, but we avoid maintaining that extra list: use Svelte-aware
// tooling, or import shared types directly from .ts files.
declare module '*.svelte' {
  import type { Component } from 'svelte';

  const component: Component;
  export default component;
}
