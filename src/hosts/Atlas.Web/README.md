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

## Configuration reference

These files configure different tools and stages. They are text source files: `.ts` means TypeScript, `.js` means JavaScript, and `.json` is structured configuration. Playwright's saved `trace.zip` is a generated archive, not a configuration file.

| File | Read by / when | What it controls |
| --- | --- | --- |
| [package.json](package.json) | npm, during installation and script execution | Dependencies, supported Node version and commands such as `dev`, `check`, `build`, `storybook` and `test:ci`. Its `prepare` script generates SvelteKit configuration after installation. |
| [package-lock.json](package-lock.json) | npm install / npm ci | Exact resolved dependency versions. Commit updates with dependency changes; regenerate through npm rather than editing manually. CI uses `npm ci` to reproduce this install. |
| [svelte.config.js](svelte.config.js) | SvelteKit and Svelte tooling | Svelte preprocessing and the static adapter. Writes application assets to `dist/` and generates the `index.html` SPA fallback. |
| [vite.config.ts](vite.config.ts) | Application development, build and preview | Loads the SvelteKit Vite plugin. Vite runs the development server and builds the frontend assets. This config belongs to the application. |
| [tsconfig.json](tsconfig.json) | TypeScript, svelte-check and editor tooling | Strict type checking, included source/config/test files and module resolution. Extends SvelteKit's generated configuration so aliases and framework types agree. |
| [src/vite-env.d.ts](src/vite-env.d.ts) | TypeScript and editor tooling | Loads Vite's asset declarations, including CSS imports. It provides types, not runtime behavior. |
| [.storybook/main.ts](.storybook/main.ts) | Storybook development and build | Finds story files, selects the SvelteKit renderer, enables addons and disables Storybook telemetry. Despite its name, it does not start the application. |
| [.storybook/preview.ts](.storybook/preview.ts) | Storybook's story-rendering environment | Imports shared styles and configures preview parameters, including accessibility behavior. This affects the stories being displayed. |
| [storybook-preview.config.ts](storybook-preview.config.ts) | Plain Vite preview, launched by Playwright | Serves the already-built `storybook-static/` directory during tests. It intentionally omits the SvelteKit plugin. It does not configure story appearance or build Storybook. |
| [playwright.config.ts](playwright.config.ts) | `npm test` | Test directory, Chromium, timeouts, CI retries, HTML reports and retained failure traces. Starts and stops both temporary servers: application on port 4173 and built Storybook on port 6006. |
| [src/routes/+layout.ts](src/routes/+layout.ts) | SvelteKit routing/rendering | Sets `ssr = false` for this client-rendered SPA. This is a route option file, distinct from the layout component's markup. |
| [src/app.html](src/app.html) | SvelteKit document generation | Outer HTML shell, language and viewport. SvelteKit fills its head/body placeholders; page and component HTML belongs in `.svelte` files. |

Repository-level files also affect this workspace:

- [AtlasWeb.yml](../../../.github/workflows/AtlasWeb.yml): installs Node/dependencies/browser libraries and runs `npm run test:ci` on relevant pushes. Saves browser failure artifacts.
- [Atlas.sln](../../../Atlas.sln): exposes the frontend files in Solution Explorer folders; it does not make npm builds part of `dotnet build`.
- [.gitignore](../../../.gitignore): excludes dependencies, generated SvelteKit files, built outputs and local browser reports/traces.

**Why there are two “preview” files:** `.storybook/preview.ts` controls the story experience; `storybook-preview.config.ts` controls only the static test server. Playwright selects the latter explicitly with `--config` so Vite does not use the application's SvelteKit configuration.

The command relationships are:

- `npm run dev` → application Vite config → SvelteKit/Svelte config → pages and components.
- `npm run storybook` → Storybook main config → preview config → isolated Svelte stories.
- `npm run test:ci` → application check/build → Storybook build → Playwright → the two temporary preview servers.

Generated `.svelte-kit/tsconfig.json` and other `.svelte-kit/` files are owned by SvelteKit. Recreate them with `npx svelte-kit sync` (also included in installation/check commands); change the project configuration rather than editing generated files.

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
