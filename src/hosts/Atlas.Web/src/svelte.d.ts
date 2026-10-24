/**
 * Fallback for TypeScript tooling that does not inspect Svelte component modules.
 * This wildcard describes only the default component export; it cannot discover
 * the different named exports provided by each .svelte file.
 *
 * Svelte files CAN also have named exports in a module script. This declaration
 * does not forbid them or restrict the application to default exports. Svelte-aware
 * tooling can resolve those exports from the actual component source.
 *
 * Default import: import Breadcrumb from './Breadcrumb.svelte';
 * Named type import: import type { BreadcrumbPage } from './breadcrumb.types';
 * Braces select a named export; they are not a nested or "sub" component import.
 *
 * For data types shared with plain .ts code, define them once in a .ts module and
 * import them directly from there. A component can re-export those same types for
 * compatibility, but importing them through .svelte still needs Svelte-aware tooling.
 * Avoid manually listing each component's named exports here: that would duplicate
 * contracts to maintain. If an IDE flags a valid named .svelte import, check its
 * Svelte language tooling rather than adding an untyped catch-all to this shim.
 */
declare module '*.svelte' {
  import type { Component } from 'svelte';

  const component: Component;
  export default component;
}
