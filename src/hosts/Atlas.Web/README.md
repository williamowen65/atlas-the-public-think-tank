# Atlas.Web: SvelteKit and Storybook

PTT-143, PTT-144 and PTT-145 deliver the foundation from [PTTD-31](https://thepublicthinktank.atlassian.net/browse/PTTD-31). SvelteKit is the selected frontend framework. Svelte components provide declarative HTML, reactive state and callbacks; SvelteKit supplies the application structure and router. [PTT-148](https://thepublicthinktank.atlassian.net/browse/PTT-148) tracks Redux Toolkit configuration separately.

## Start here

Install Node.js 24 LTS. From this directory:

```sh
npm ci
npm run dev
```

Open http://127.0.0.1:5173. The home page demonstrates the shared button; the navigation link opens `/about` through SvelteKit's client router. Browser Back and direct entry to `/about` are covered by browser tests.

In another terminal:

```sh
npm run storybook
```

Open http://127.0.0.1:6006. Foundation / ActionButton shows the production component directly with enabled, disabled and long-label inputs. Examples / ActionButton adds a visible callback result. No API, SQL, credentials or session is required.

## Where the markup lives

| File | Responsibility |
| --- | --- |
| `src/lib/components/ActionButton.svelte` | Actual reusable button HTML and callback contract |
| `src/lib/components/ActionButton.stories.ts` | Direct component scenarios; inputs only, no duplicate HTML |
| `src/lib/components/ActionButtonExample.svelte` | Small demo composition with reactive status text |
| `src/lib/components/ActionButtonExample.stories.ts` | Enabled/disabled scenarios for that demo |
| `src/routes/+page.svelte` | Home page imports the same demo, which imports the shared button |
| `src/routes/+layout.svelte` | Shared navigation, styles and page outlet |
| `src/routes/about/+page.svelte` | Second route proving SvelteKit navigation |
| `src/app.html` | SvelteKit's HTML document shell |
| `.storybook/main.ts` | Storybook's SvelteKit adapter, story paths and addons |
| `.storybook/preview.ts` | Shared Storybook styles and preview settings |

There is no handwritten DOM assembly or application `src/main.ts`. SvelteKit handles bootstrapping. The remaining `.storybook/main.ts` is configuration, not the page entry point. Solution Explorer mirrors the source folders; install the Svelte editor extension for `.svelte` language support where your IDE supports it. `npm run check` remains the authoritative component/type check.

A component owns its HTML. A story supplies a named setup through `args`; it does not recreate production markup. A decorator wraps stories with shared layout/context when needed; the first button needs none. The status message belongs to the demonstration, not the production button. Its `$state` is local example state, not Redux.

## App, API and deployment boundaries

Atlas.Web and Atlas.Api remain sibling projects. Atlas.Api continues to own backend and Identity workflows. Storybook is an independently runnable developer/test surface inside the web workspace, not a deployed runtime dependency.

This slice uses SvelteKit's static adapter with client rendering (`ssr = false`). `npm run build` outputs `dist/`, including `index.html` as the SPA fallback. It needs no separate production Node server. The likely MVP public origin remains `/` for the web and `/api` for the backend. Production hosting must serve real assets first and apply the SPA fallback only to frontend routes, never swallow `/api` errors or missing asset requests. That hosting integration is not implemented here.

Pure client rendering has startup and SEO/share-preview tradeoffs. Revisit prerendering, server rendering and per-node social metadata before public launch; choosing SvelteKit leaves those options available but this setup does not implement them. No backend is duplicated in SvelteKit and no API/CSRF/cookie behavior is changed. PTT-147 covers real API integration and secure local HTTPS/proxy behavior.

## Build and test

```sh
npx playwright install chromium
npm run test:ci
```

On Linux with missing browser libraries, use `npx playwright install --with-deps chromium`. Commands:

- `npm run check`: generate SvelteKit types and run Svelte/TypeScript diagnostics.
- `npm run build`: check and build the static application.
- `npm run build-storybook`: build the isolated Svelte workbench.
- `npm test`: serve both built outputs temporarily and run Chromium checks.

Rebuild after editing source because tests intentionally target built output. Storybook's static preview uses its own plain Vite config; the web preview uses SvelteKit's route fallback.

The six browser checks cover enabled keyboard activation, disabled behavior, long text on narrow screens, shared-component web composition, initial automated accessibility, and routing (client navigation, Back, direct entry). Accessibility automation is partial; manual keyboard and assistive-technology review is still necessary. These fixture tests do not replace future complete user journeys against the real API.

GitHub Actions runs on relevant pushes only, including pushes to PR branches. It installs locked dependencies and Chromium, checks/builds both surfaces and runs the browser suite. Failure reports and traces are retained for seven days. Open a report with `npx playwright show-report` or a trace with `npx playwright show-trace <trace.zip>`. No public Storybook hosting, SQL, external secrets or paid visual service is required. Storybook telemetry is disabled.

The earlier HTML-renderer foundation verified that a deliberately incorrect assertion fails CI ([negative verification run](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/39465964351)); that incorrect assertion was restored. The current Svelte conversion preserves those behavioral checks and adds routing coverage.

## References

- https://storybook.js.org/docs/get-started/frameworks/sveltekit
- https://svelte.dev/docs/kit/routing
- https://svelte.dev/docs/kit/single-page-apps
- https://svelte.dev/docs/svelte/$state
