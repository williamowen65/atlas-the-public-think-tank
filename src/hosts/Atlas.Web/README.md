# Atlas.Web: SvelteKit and Storybook

PTT-143, PTT-144 and PTT-145 deliver the foundation from [PTTD-31](https://thepublicthinktank.atlassian.net/browse/PTTD-31). SvelteKit is the selected frontend framework. Svelte components provide declarative HTML, reactive state and callbacks; SvelteKit supplies the application structure and router. [PTT-148](https://thepublicthinktank.atlassian.net/browse/PTT-148) tracks Redux Toolkit configuration separately.

## Learning guides

- [SvelteKit orientation](docs/SVELTEKIT.md): route folders, plus files, dynamic pages, layouts, rendering, SSR and our static deployment choice.
- [Storybook workbench](docs/STORYBOOK-WORKBENCH.md): styles/layout, components/compositions, global themes and density, legacy dial inspection and voting prototype limits.

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

Open http://127.0.0.1:6006. Components / ActionButton is the single button story group. There is one Playground story; use Controls to change label and disabled state. No API, SQL, credentials or session is required.

## Styling with Sass

Use SCSS (Sass with braces and semicolons) for styles. The `sass` development dependency and existing `vitePreprocess()` in `svelte.config.js` compile it automatically. `npm run dev` and `npm run storybook` watch changes and update the preview; the existing build and CI commands compile production CSS. Do not run a separate Sass watcher, generate sibling `.css` files, or commit generated CSS.

- ActionButton owns its button appearance and disabled state; the typography reset uses ActionButton rather than a global button class.
- Component-specific styles stay in that component's `<style lang="scss">` block. Svelte scopes their selectors so they do not affect unrelated components. No stylesheet import is needed for this block.
- Shared tokens and application-wide rules live in `src/styles.scss`. The app layout and Storybook preview each import that source as the entry point for their respective build.
- Use one declaration per line, blank lines between rules, shallow nesting for related selectors, and `&` for states such as `&.dragging` or `&:focus-visible`. Nest compact overrides under `.compact`.
- Keep runtime theme/density values as CSS custom properties. Sass runs at build time; CSS variables still respond to the Theme and Density controls in the browser.

A style block in a Svelte source file is compiler input, not markup repeated for each mounted component. Production builds extract and bundle its scoped CSS into shared assets; rendering twenty voting dials does not emit twenty copies of the stylesheet. Development tooling may inject/update a style tag for hot reload, shared across instances. The multiple-dial browser test also checks that no style blocks appear inside the rendered dials.

See the [Vite preprocessor guide](https://vite.dev/guide/features.html#css-pre-processors) and [Svelte Vite preprocessing guide](https://github.com/sveltejs/vite-plugin-svelte/blob/main/docs/preprocess.md).

## Where the markup lives

| File | Responsibility |
| --- | --- |
| `src/lib/components/ActionButton.svelte` | Actual reusable button HTML and callback contract |
| `storybook/ActionButton/ActionButtonExample.svelte` | Small demo composition with reactive status text |
| `storybook/ActionButton/ActionButton.stories.ts` | Single Playground story with editable Controls |
| `src/routes/+page.svelte` | Home page imports the production button directly and owns its page state |
| `src/routes/+layout.svelte` | Shared navigation, styles and page outlet |
| `src/routes/about/+page.svelte` | Second route proving SvelteKit navigation |
| `src/app.html` | SvelteKit's HTML document shell |
| `.storybook/main.ts` | Storybook's SvelteKit adapter, story paths and addons |
| `.storybook/preview.ts` | Shared Storybook styles and preview settings |

There is no handwritten DOM assembly or application `src/main.ts`. SvelteKit handles bootstrapping. The remaining `.storybook/main.ts` is configuration, not the page entry point. Solution Explorer mirrors the source folders; install the Svelte editor extension for `.svelte` language support where your IDE supports it. `npm run check` remains the authoritative component/type check.

A component owns its HTML. A story supplies a named setup through `args`; it does not recreate production markup. A decorator wraps stories with shared layout/context when needed; the first button needs none. All story files and Storybook-only Svelte wrappers live under `storybook/`, outside production `src/`. The app imports only the reusable button, never a Storybook example. `.storybook/` holds tool configuration; `storybook/` holds the examples themselves. The status message belongs to the demonstration, not the production button. Its `$state` is local example state, not Redux.

Storybook uses the metadata title `Components/ActionButton` for the sidebar group. Each group now contains one Playground story. Variations are selected in Controls rather than duplicated in the sidebar. The typography playground has an explicit reset-to-code-defaults button; other argument edits can be reset in the Controls panel. Refreshing may retain edited story arguments and is not a reset. The top-level paths are Styles and Layout, Components and Compositions; no separate Foundation/Examples groups remain.

## Understanding package.json

`package.json` is npm's project manifest. It records the package identity, module format, runtime requirements, dependencies and named commands. JSON does not support comments, so explanations live here rather than being inserted into the manifest.

| Setting | Why it is here |
| --- | --- |
| `name: "@atlas/web"` | Identifies this frontend workspace. `@atlas` is the package-name scope; it does not register an npm organization or publish anything. |
| `version: "0.0.0"` | Placeholder version for this application workspace. It is not the version of SvelteKit, the API or a deployed release. |
| `private: true` | Prevents accidental publication of this workspace through npm. It does **not** make the repository private, restrict website access or prevent deployment of built files. |
| `type: "module"` | Makes Node treat this package's `.js` files as ES modules, so files such as `svelte.config.js` can use `import` and `export`. TypeScript files are handled by their tooling. This setting does not select a UI framework or a rendering mode. |
| `engines.node: ">=22.12.0"` | Declares the minimum supported Node version. Our documented/CI baseline is Node 24 LTS. The field does not install Node; npm may warn rather than refuse an unsupported version, depending on its configuration. |
| `scripts` | Names repeatable commands. npm makes local dependency executables available to them, so `vite` and `svelte-check` do not need global installations. |
| `devDependencies` | Tools and libraries needed to develop, check, build and test this static frontend. Some, such as Svelte, contribute code to the built browser assets; “dev” does not mean none of their code can reach the browser. The deployed static output does not run npm or need a production `node_modules` directory. |

The lockfile records exact resolved versions. In the manifest, an exact version pins that request; `^` generally permits compatible minor/patch updates for versions at least 1.0, while `~` generally permits patch updates. Use normal dependency-update review and commit the resulting lockfile.

### When to run each script

Run these commands from `src/hosts/Atlas.Web`. From the repository root, the equivalent is `npm --prefix src/hosts/Atlas.Web run <script>`. The `test` script also has the shorthand `npm test`.

| Script / command | When to run it | What happens / when it finishes |
| --- | --- | --- |
| `npm run dev` | While developing pages, layouts, routing or checking components inside the actual app. | Starts the SvelteKit development server, normally on port 5173, with updates as you edit. It keeps running until Ctrl+C. It is not a production build or a test run. |
| `npm run storybook` | While developing or reviewing isolated component states and controls. | Starts Storybook on port 6006, keeps running and updates stories as you edit. Open the URL manually because `--no-open` disables automatic browser launch. Telemetry is disabled. |
| `npm run check` | After component/TypeScript changes, before committing, or when investigating editor diagnostics. | Generates SvelteKit types, then checks Svelte and TypeScript using `tsconfig.json`. Finishes with diagnostics and a success/failure exit code. It does not build output or run browser tests. |
| `npm run build` | To verify the application can be packaged, or before browser tests after application changes. | Runs `check` first; only if it passes does Vite build the static app into `dist/`. Finishes rather than keeping a server running. |
| `npm run build-storybook` | To verify stories can be packaged, or before browser tests after component/story changes. | Builds static Storybook into `storybook-static/`, with telemetry disabled. Finishes without starting a server or deploying anything. |
| `npm test` | To run browser checks when both built outputs already reflect the source being tested. | Playwright starts the temporary app, built Storybook and development Storybook servers, runs Chromium assertions, writes reports/traces as configured and stops the servers. It does **not** rebuild those outputs. |
| `npm run test:ci` | Before submitting a frontend change, for a complete local validation pass, and in GitHub Actions. | Sequentially builds/checks the app, builds Storybook and runs the browser tests. `&&` stops the sequence at the first failing command. It finishes with an exit code suitable for CI. |
| `prepare` lifecycle / `npm run prepare` | Normally runs automatically during a standard local `npm install` or `npm ci`. Run it manually only when you want to regenerate SvelteKit metadata. | Runs `svelte-kit sync` to generate framework configuration/types in `.svelte-kit/`. It does not perform the full type check, build or browser tests. `npm run check` also syncs, so manual execution is usually unnecessary. |

`--host 127.0.0.1` binds development servers to the local machine rather than exposing them to other devices. Playwright also uses local addresses. Both interactive Storybook and its test server use port 6006, so stop the interactive Storybook server before running browser tests; Playwright deliberately refuses to reuse an existing server.

### Typical command choices

- **First checkout or after pulling dependency changes:** `npm ci`. It installs the lockfile's dependencies, replaces the existing install and runs the prepare lifecycle unless lifecycle scripts are disabled. Use `npm install` when intentionally changing dependencies and updating the lockfile.
- **Work on a component:** `npm run storybook`; optionally also `npm run dev` in another terminal to inspect app composition.
- **Quick validation while coding:** `npm run check`.
- **Full validation before review:** stop interactive Storybook, install Chromium once with `npx playwright install chromium`, then run `npm run test:ci`.
- **Repeat browser tests without source changes:** `npm test`. After changing components/pages/stories, rebuild the affected outputs or use `test:ci` to avoid testing stale files.

These scripts do not start Atlas.Api, connect to SQL, configure Redux, or deploy the application. Those remain separate workflows.

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
| [playwright.config.ts](playwright.config.ts) | `npm test` | Test directory, Chromium, timeouts, CI retries, HTML reports and retained failure traces. Starts and stops temporary servers: application on port 4173, built Storybook on port 6006 and development Storybook on port 6007 for its renderer regression check. |
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
- `npm run test:ci` → application check/build → Storybook build → Playwright → the preview servers and development Storybook regression server.

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
- `npm test`: serve both built outputs and development Storybook temporarily, then run Chromium checks.

Rebuild after editing source because tests intentionally target built output. Storybook's static preview uses its own plain Vite config; the web preview uses SvelteKit's route fallback.

Browser checks cover enabled keyboard activation, disabled behavior, long text on narrow screens, shared-component web composition, initial automated accessibility, and routing (client navigation, Back, direct entry). Accessibility automation is partial; manual keyboard and assistive-technology review is still necessary. These fixture tests do not replace future complete user journeys against the real API.

GitHub Actions runs on relevant pushes only, including pushes to PR branches. It installs locked dependencies and Chromium, checks/builds both surfaces and runs the browser suite. Failure reports and traces are retained for seven days. Open a report with `npx playwright show-report` or a trace with `npx playwright show-trace <trace.zip>`. No public Storybook hosting, SQL, external secrets or paid visual service is required. Storybook telemetry is disabled.

The earlier HTML-renderer foundation verified that a deliberately incorrect assertion fails CI ([negative verification run](https://github.com/williamowen65/atlas-the-public-think-tank/actions/runs/39465964351)); that incorrect assertion was restored. The current Svelte conversion preserves those behavioral checks and adds routing coverage.

## References

- https://storybook.js.org/docs/get-started/frameworks/sveltekit
- https://svelte.dev/docs/kit/routing
- https://svelte.dev/docs/kit/single-page-apps
- https://svelte.dev/docs/svelte/$state
