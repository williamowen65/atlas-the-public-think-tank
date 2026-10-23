# Reading Atlas.Web Svelte files

Start with `src/app.html` (document shell), then `src/routes/+layout.ts` (route options), `+layout.svelte` (shared header/footer), and `+page.svelte` (the Discover page). SvelteKit supplies the page and any nested layouts through the layout's `children` snippet; `{@render children()}` inserts them into the shared shell. Nested routes can add their own layouts.

## State and props

- `$props()` receives values from the parent. A component passes values to children through attributes, including shorthand such as `{displayName}`.
- `$state(...)` declares reactive state owned by that component instance. Changing it updates the parts of the rendered UI that depend on it.
- `$derived(...)` and `$derived.by(...)` compute values from reactive dependencies; they recalculate when those dependencies change.
- `$app/state` supplies SvelteKit-managed route information. Its `page` export includes the current URL and route data; it is separate from Atlas's own shared state and any Redux store.
- `$lib` is an import alias for `src/lib`, not a state container.

## Component lifecycle and DOM events

Lifecycle hooks describe when component code runs. They are different from DOM event handlers such as `onclick`, `onsubmit`, or `onkeydown`.

| API | When and why Atlas uses it |
| --- | --- |
| `onMount(callback)` | Runs after the component mounts in the browser. Use it for browser event listeners or initial session reads. A synchronous callback can return a cleanup function. |
| Mount cleanup / `onDestroy(callback)` | Release listeners, timers and requests when a component is removed. `onDestroy` can also run during server rendering. The root layout uses the function returned by `onMount`. |
| `$effect(callback)` | Runs in the browser after mounting and again when reactive values read synchronously by the callback change. Its cleanup runs before rerunning and when the component is removed. Use it for side effects, not ordinary computed values. |
| `tick()` | Returns a promise that resolves after pending state updates have been applied. Use it before focusing or measuring newly rendered elements. |

The root layout's mount callback reads the existing authentication session; it does not force a login. `authenticated` and `displayName` become header props. The custom `atlas-session-changed` browser event asks the layout to refresh after sign-in/out. An `active` flag prevents late requests from changing an unmounted layout, and cleanup removes the event listener.

## Formatting

Run `npm run format` to format Svelte source/stories and the root route options, or `npm run format:check` to check formatting. Imports belong together at the start of each script. Comments explain intent and lifecycle boundaries rather than restating each line. Plain styles remain CSS; existing SCSS stays SCSS where it uses nesting or other Sass features.

References: [Svelte lifecycle](https://svelte.dev/docs/svelte/lifecycle-hooks), [Svelte effects](https://svelte.dev/docs/svelte/$effect), [SvelteKit routing](https://svelte.dev/docs/kit/routing).
