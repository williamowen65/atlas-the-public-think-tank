// Fallback for TypeScript tooling without Svelte support. It describes only the
// default component export; named exports (types or values) need Svelte-aware tooling.
// Svelte itself supports both. Shared types can be imported from .ts files instead.
declare module '*.svelte' {
  import type { Component } from 'svelte';

  const component: Component;
  export default component;
}
