# SvelteKit orientation for Atlas developers

Svelte describes reusable components with declarative markup, reactive state and scoped styles. SvelteKit adds routing, page/layout conventions, data loading, rendering options and build adapters. Atlas.Api remains our ASP.NET backend; choosing SvelteKit does not move business rules or Identity into the frontend.

## Route folders and the plus sign

Routes live in `src/routes`. Folder names create URL segments; a `+page.svelte` makes a visible page at that location. The plus prefix identifies a framework-recognized file, not an operator. Ordinary reusable components have ordinary names under `src/lib/components`; importing one does not create a URL.

| Example path | URL or responsibility |
| --- | --- |
| `src/routes/+page.svelte` | `/`, the current integration example |
| `src/routes/about/+page.svelte` | `/about`, the current second page |
| `src/routes/discover/+page.svelte` | Future `/discover` page; illustrative, not implemented |
| `src/routes/nodes/[id]/+page.svelte` | Future `/nodes/123`, `/nodes/456`, etc. |
| `src/routes/+layout.svelte` | Shared navigation and surrounding markup for descendant pages |
| `src/routes/communities/+layout.svelte` | Example nested layout surrounding pages in that subtree |

A folder alone does not create a page. Several route folders can share a layout without sharing a page implementation. Regular anchors such as `<a href="/about">About</a>` work with SvelteKit client navigation for internal routes. Real navigation, Back/Forward and direct URLs are tested in the running app, not inferred from a Storybook fixture.

## Special files

| File | Purpose | Execution / scope |
| --- | --- | --- |
| `+page.svelte` | Page markup composed from reusable components | Current route |
| `+layout.svelte` | Shared UI and page outlet | Route subtree |
| `+page.ts` | Universal page `load` function or page options | Server during SSR when enabled; browser during client navigation, subject to rendering options |
| `+layout.ts` | Shared universal data loading or inherited page options | Route subtree |
| `+page.server.ts` | Server-only loading and form actions | Requires server execution (or applicable build-time prerender loading); not a browser secret store |
| `+layout.server.ts` | Shared server-only loading | Server/build execution as applicable |
| `+error.svelte` | Error boundary UI at that route level | Errors handled by the relevant boundary |
| `+server.ts` | HTTP handlers such as GET/POST | SvelteKit endpoint; not required to duplicate Atlas.Api |
| `src/app.html` | Outer HTML document shell with SvelteKit placeholders | Framework-controlled document generation |

Layout markup receives the routed page as a child snippet:

```svelte
<script lang="ts">
  import Header from '$lib/components/Header.svelte';
  let { children } = $props();
</script>

<Header />
<main>{@render children()}</main>
```

This Header is an illustrative future component, not part of the current source. Layouts nest, so community navigation can surround community pages while the root header remains around the entire app.

## Dynamic pages and data

`[id]` captures a URL segment. One `nodes/[id]/+page.svelte` can render every node. For `/nodes/123`, `params.id` is the string `123`; it is input, not proof the node exists or that access is authorized.

An illustrative universal loader for the current client-rendered approach:

```ts
// Future src/routes/nodes/[id]/+page.ts
import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ params, fetch }) => {
  const response = await fetch(`/api/nodes/${encodeURIComponent(params.id)}`);
  if (response.status === 404) error(404, 'Node not found');
  if (!response.ok) error(response.status, 'Unable to load this node');
  return { node: await response.json() };
};
```

This demonstrates routing, not an implemented Atlas client contract. PTT-147 owns the typed API adapter and session/error handling. The page passes data into a NodeCard; the card does not need to know the URL or perform its own fetch. Loading, empty, missing, hidden and error states can also be explored with fixtures in Storybook. Authorization remains enforced by the API.

`$lib` resolves to `src/lib`. Generated `./$types` describe a route's parameters and loaded data; `svelte-kit sync` generates them. Do not edit `.svelte-kit` files.

## Rendering: CSR, SSR, prerendering and hydration

| Mode | When useful HTML is generated | Production requirements |
| --- | --- | --- |
| Client-side rendering (CSR) | Visitor's browser after JavaScript loads | Static assets plus frontend route fallback |
| Prerendering / static generation | Build time | Generated HTML and assets; new dynamic data/pages need a new build or another rendering path |
| Server-side rendering (SSR) | Request time | SvelteKit-compatible server runtime and adapter |
| Hydration | Browser attaches interactivity to server/build-generated HTML | Client JavaScript matching the rendered page |

SSR and client-side routing can coexist. SSR normally prepares the first requested page; after hydration, later internal navigation can remain client-side. Dynamic parameters do not require SSR, and server rendering does not eliminate API calls or database work.

## Why Atlas currently sets ssr = false

The root `src/routes/+layout.ts` exports `ssr = false`, inherited by descendant pages unless overridden. Combined with `adapter-static`, this produces a client-rendered SPA in `dist/` and an `index.html` fallback. We selected it to keep the starting frontend deployable as static files alongside Atlas.Api, not because client rendering always starts faster.

The browser initially receives the app shell and then renders content. This has first-load, JavaScript-dependency, SEO and social-preview tradeoffs. Atlas's per-node sharing metadata needs an explicit production decision before launch.

Changing the flag alone does not give our static deployment request-time SSR. To enable it we must select a server-capable adapter and runtime, arrange same-origin access to Atlas.Api, review cookie forwarding and data loading, guard browser-only code, isolate Redux state per server request and test hydration/direct entry. The framework handles the rendering mechanism; hosting still runs and pays for the compute. A public page can instead be prerendered if its URLs/data are known at build time. Personal/private content must not be baked into public build output.

Production fallback routing must preserve `/api` responses and real missing asset responses. The current browser tests verify SvelteKit preview; they do not establish an Azure/ASP.NET production hosting configuration.

## Components, stories and tests

Production components live in `src/lib/components`; their HTML stays in `.svelte` files. Storybook fixtures and stories live in `storybook/` and import those components. Components do not import stories. `$props` declares inputs, `$state` is reactive local state and callbacks notify the parent. Redux application state is tracked in PTT-148; local fixture state is not an implemented Redux store.

Storybook supports interaction tests through play functions and its testing integrations. Our current suite uses Playwright against built stories and the running SvelteKit app. Isolated stories verify component states; compositions verify interactions among components; actual app tests verify real routing and eventually API/session behavior. None is a substitute for the others.

## Primary references

- https://svelte.dev/docs/kit/routing
- https://svelte.dev/docs/kit/load
- https://svelte.dev/docs/kit/page-options
- https://svelte.dev/docs/kit/single-page-apps
- https://svelte.dev/docs/kit/adapter-static
- https://svelte.dev/docs/kit/adapter-node
- https://svelte.dev/docs/svelte/scoped-styles
- https://storybook.js.org/docs/writing-tests/interaction-testing
