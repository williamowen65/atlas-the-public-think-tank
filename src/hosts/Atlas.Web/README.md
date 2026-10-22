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

## Preview real local nodes

Once your existing Atlas development database is configured, start both hosts from this directory:

```powershell
npm ci
npm run storybook:api
```

This starts Atlas.Api with its HTTPS launch profile and Storybook together, with labeled output. Ctrl+C stops both. If either process exits, the other is stopped too. If you already started Atlas.Api in Visual Studio, use `npm run storybook` instead to avoid starting a second API on the same port.

Open http://127.0.0.1:6006, then **Compositions → Atlas node → Playground**.
The node-card preview always loads public Atlas API data: search, paging, node selection,
Refresh and ancestor navigation. There is no fixture source or simulated node-card mode.
The presentation Control compares the ribbon card with the original card using the same
live reader. Clicking an ancestor fetches that exact ID through Discovery, even outside
the current page/search; Back to Discovery restores the search. Archived ancestors can
be viewed, while hidden or missing nodes produce an unavailable message with Retry.

No login is needed. Anonymous dial attempts briefly show the attempted selection, then
return to neutral (unrated, centered on 5). They never cast a database vote. The standalone
VotingDial story retains its isolated accepted/anonymous/failed outcome simulations;
these make no API requests. Node-card comments and connected nodes currently display API
summaries; reactions use individual public vote totals from the reactions endpoint; no discussion is fabricated. Discovery descriptions remain plain text rather
than a complete content-block renderer.

One-time prerequisites are the .NET SDK used by Atlas.Api, your existing migrated SQL database and shared connection-string secret, and a development HTTPS certificate. Follow [the API setup guide](../../../docs/api/README.md#1-start-the-server) if these are not already configured. The launcher does not migrate, seed or reset your database.

The default API origin is `https://localhost:7240`. For a different local port, set this **before** starting Storybook (the proxy reads it at startup):

```powershell
$env:ATLAS_STORYBOOK_API_URL = "https://localhost:7240"
npm run storybook:api
```

The variable changes the proxy destination; configure the API's listening port to match separately. Only loopback HTTP(S) origins are accepted. Storybook's development server proxies anonymous GET requests from `/atlas-preview-api/nodes` to `/api/nodes`; it rejects writes and strips authentication cookies/headers and response cookies. It accepts the local API's development certificate without changing browser/production TLS settings. No CORS changes or login secrets are needed. This proxy is absent from static Storybook builds: use the development command for live data. A static node-card preview also requests the API, so it needs a host that routes the preview endpoint. CI intercepts those requests using Playwright. When API loading fails, the preview gives setup guidance and Retry instead of silently substituting fake content.

The reusable read adapter lives in `src/lib/api/discovery.ts`; the Storybook-only loader owns selection, request cancellation, timeout and loading/error states. Browser CI uses intercepted contract-shaped responses and verifies that requests stay read-only; it does not connect to your SQL Server. A real database smoke check must be done with your local API running. Authenticated previews and durable voting remain a later, explicit mode tied to a selected node.

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

### Breadcrumb and node card presentations

`Components/Breadcrumb → Playground` isolates the legacy-inspired arrow ribbon. Use
Depth to explore empty, short and overflowing paths. Items run from oldest to newest;
the rightmost item is the newest ancestor. The ribbon scrolls to the right before it
becomes visible. Scrolling to the left edge loads the next three older ancestors.
Prepending preserves the visible position; a new path resets to the newest item.
Resizing keeps the newest item in view when already scrolled to that end. Older ancestors remain accessible
through native horizontal scrolling and keyboard focus. CSS pseudo-elements form the
arrows, with four alternating colors and a visible focus treatment. The native scrollbar
is hidden while touch, trackpad and keyboard scrolling remain available. The ribbon
touches the card’s left inner border; header/body spacing stays local to those sections.
The ribbon presentation puts the node type before the title in the same heading and
keeps reaction pills and the labeled community inside the main body beside/under
the intro text and dial. Comments and connected-node text form a right-aligned stack
in that same body; the ribbon candidate has no separate bottom sections.

`Compositions/Atlas node → Playground` has one **presentation** Control:

- `context-below`: type badge/header and plain context below the description.
- `ribbon`: breadcrumb above the header, with the dial beside the description.
- `ribbon-responsive`: the ribbon arrangement on mobile; at 48rem and wider the
  dial shares the header area, with header/content indented beside it.

`NodeCard.svelte` selects the version; its default remains `ribbon`. These descriptive
names do not indicate which version is active. Each file in `components/node-card-versions/`
owns its complete markup, local state and scoped SCSS. There is no shared NodeCardLayout
or layout wrapper. The versions reuse VotingDial, Breadcrumb and shared data/props types;
API loading, navigation and voting feedback remain in their caller. This deliberately
allows designs to diverge independently, with some repeated markup/styles between files.
The responsive version is an additional candidate, not a replacement for the ribbon.

Breadcrumb items accept a title and optional node ID, oldest first. The parent owns
navigation; live ancestor clicks select the exact requested node through Discovery. The shared
component accepts an asynchronous `loadOlder` callback and `hasMore` flag, prevents
concurrent loads, stops at the root or repeated ancestors, and offers retry on failure.
A Load older ancestors button also supports keyboard use and paths too short to scroll.

The local API adapter consumes Discovery's structured `breadcrumbSegment` (reversing
its nearest-first order) and reads older pages from the existing public
`GET /api/nodes/{id}/ancestors` endpoint. The development proxy admits only node-list
and ancestor/reaction GETs, strips credentials, and continues to reject writes and
other routes. Discovery now supports an optional NodeId filter, included in its cache
key and applied before ranking/paging with all existing visibility rules. Exact-ID
reads include archived nodes and do not inherit the search text. Older API responses
without a structured segment retain their formatted text summary. The standalone breadcrumb
story simulates bounded asynchronous pages, with Depth controlling the entire route.

Legacy references: `legacy/www.thepublicthinktank.com/Views/Shared/_Breadcrumb.cshtml`
and `wwwroot/css/breadcrumb.css`. The new component handles updates and resizes rather
than relying only on a document-load event. Browser coverage checks right alignment,
scrolling/focus, mobile containment, navigation, accessibility and the original candidate.

The dedicated authored short-description idea is tracked in [PTTD-38](https://thepublicthinktank.atlassian.net/browse/PTTD-38). Preview text still comes from content blocks. Reaction pills display total vote counts, not net scores; unused choices are omitted. The ribbon card renders only structured reaction pills; empty/loading/failed reaction lists reserve no reaction row. Failures show a retry note outside the card rather than a comma-separated summary pill. Comments and connected-node counts share one plain-text line (wrapping on narrow screens), without a Connected nodes label.

Ribbon card spacing uses a 1.25rem (20px) side inset, also used between breadcrumb and title. Narrow screens use .75rem (12px). The responsive desktop dial column sizes to its content with the same 1rem text gap as the float layout (about 22px from the visible selector including its inner padding). Comments are capitalized and separated from connected-node counts with the same middle dot treatment.

### Concise node introductions

Cards use Discovery's `shortDescription` (Graph-owned, at most 280 UTF-16 code units), never a content-block excerpt. They display it in full without line clamping, with the existing ActionButton centered against the full card on a new line directly after the text, inside the description flow. A shared resize action compensates for the dial column without moving the button below it. `onReadMore(nodeId)` is caller-owned navigation to the future detail page; when absent, the action is disabled and never expands the card. Storybook currently has no detail-page destination. Full content blocks remain separate API resources. Update the API/database using [the pending short-description migration instructions](../../Atlas.Persistence/README.md#node-short-description-pending-migration) and reseed local demo data to preview the authored introductions; old rows default to an empty introduction.

### Split details card experiment

Select `ribbon-split` in the Atlas node story's presentation Control (available alongside the other versions). This standalone version keeps the responsive dial/header, moves community into the author/date byline, splits reactions and comments/node-type counts into two wrapping halves below the description, and places the shared Read more ActionButton in a full-width centered bottom row. With no reactions their container is omitted. Existing versions remain selectable, and NodeCard's application default remains `ribbon`.

### Corner action card experiment

`ribbon-corner` is the initial Atlas node Storybook selection. It preserves the split version's community byline and wrapping reaction/count groups, but floats the shared Read more ActionButton at the bottom-right of the description. A resize action gives short descriptions enough height to reach the dial's bottom, keeps text top-aligned, and limits the float's text exclusion to the action's actual footprint with CSS shape-outside. The last text line can share space with the action; the paragraph contains the float so comments and node counts stay below it. The action has no full-width bottom row. `ribbon-split` and all earlier versions remain selectable.

## PTT-146: shell and scrolling feed prototypes

Three separate stories each expose one Playground: **Layout/Header**, **Layout/Footer**, and **Compositions/Feed**. Header/Footer are shared presentational components with wrapping navigation. The isolated feed story uses live anonymous Atlas API data, without a surrounding app shell or Storybook node fixtures.

`NodeFeed.svelte` accepts display items, loading/error/end state and callbacks. It contains no fetch/session/database logic. An IntersectionObserver requests another page within 240px of the bottom; the always-available Load more button supports keyboard use and browsers without observer-triggered loading. Errors suspend automatic loading until Retry. The caller-owned guard prevents overlapping requests; successful pages append in order and deduplicate IDs. Existing cards remain mounted. There is no virtualization or durable feed cursor yet; the current page-number API can change order while people vote.

`LiveNodeFeed.svelte` owns bounded requests, page position, cancellation, safe Discovery adaptation and optional reaction reads. Hidden cards are filtered before rendering, empty pages can continue to the next page, failed pages retry the same number, and pagination stops at `hasNextPage = false`. Reactions do not prevent cards from appearing; failures are noted per card. Votes and comment writes are not connected in the feed prototype. Read more and ancestor links open node pages; older breadcrumb segments can load using the existing public reader.

The SPA root composes those same components. `/components` preserves the original action-button integration example. Run `npm run dev` alongside Atlas.Api to browse the feed in the SPA, or `npm run storybook:api` for the isolated stories. Both development servers use the existing anonymous loopback proxy and GET allowlist. Static hosting still needs a separately configured API/reverse proxy; Vite's development proxy is not a production service.

Browser CI intercepts API HTTP responses (no backend/database required) to verify scroll append, deduplication, moderation filtering, failure/retry, end-of-feed, narrow containment and shared SPA composition. These checks do not prove live backend ranking stability, authenticated actions, production API hosting, or large-feed performance. The header/footer are layout prototypes, without new account/session behavior.

## Shared style experiments in Storybook

Header and Footer appear under `Layout/` (the slash in their story title defines the navigation group). `Styles and Layout/Typography` is a playground and spacing reference. Its Controls include font family, base font size, line height, heading scale, paragraph/heading gaps, shared spacing, normal/narrow card padding, dial/text gap, corner radius, and eight color tokens.

Changing these Controls publishes a Storybook global and applies shared CSS variables across components and compositions as you switch stories in the same Storybook session. Font size scales rem-based measurements. Card inset and ribbon/title spacing share the same padding token; narrow card padding remains separately adjustable. Components with intentionally fixed local dimensions (such as dial row geometry) retain those dimensions.

Empty color Controls inherit the Theme toolbar palette. Explicit colors override Theme until cleared or reset; explicit spacing overrides Density. **Reset styles to code defaults** restores every style Control, including clearing color overrides. Storybook's globals and per-story Controls are exploration state, not edits to source SCSS or the separate SPA. Promote an approved palette/spacing/font into shared SCSS to make it an app default. The specimen records established card/spacing conventions and notes color pairs to check in the Accessibility panel.

## Node pages and creation drafts

**Components/Node form** loads the live, nonarchived node-type catalog. The shared `NodeForm` accepts root/parent context, an optional selected type and preview/submit callbacks. It collects a title (450), authored short description (280), Markdown content (32000), a type and requested response types. Any catalog type may be selected, even when the parent did not request it. A new-type option records a local name/description draft. The form is an editable card: node type comes before the title, the importance dial is disabled, and Markdown renders automatically as you type. The footer contains a Requested contributions selector with removable selections. Child creation appears inside the node page above its child feed, retains the known ancestor/parent breadcrumb, and scrolls to and focuses the type selector after loading. Community selection is not connected yet. Create node remains disabled until an authenticated submit callback is connected; no node/type writes or authentication bypass are introduced.

**Compositions/Node page** defaults to the first public Discovery result; its nodeId Control selects a specific node. Read more from the node/card and feed stories navigates to the matching Storybook URL, preserving global URL parameters. The SPA equivalents are `/nodes?id=<id>` and `/new`; the shared header links to root creation. Storybook remains a component workbench, not the deployment target.

`NodePage` is presentational. `LiveNodePage` loads the visible card, metadata, content blocks, reaction totals and type catalog with cancellation. Content renders as escaped text with a limited Markdown subset (paragraphs, headings, lists and fenced code), images and safe HTTP(S) links/video links. Raw HTML is not executed. Poll/chart blocks currently have text placeholders rather than interactive renderers. Hidden/unavailable nodes show a retryable notice, without fetching or rendering original hidden content.

The page has keyboard-accessible Comments/type tabs. Comments use the existing keyset-paged API, including lazy reply expansion and retry. Every active catalog type has a child tab, with requested types first; Add child opens a parented form preselected to that tab. Child feeds reuse the scrolling feed and Discovery's new optional `ParentNodeId` filter combined with `NodeTypeId`. Filtering happens before counting/ranking/paging, cache keys include the parent, and moderated parents cannot expose a child feed through that context. No schema changes or migrations are needed.

The development proxy adds only anonymous GET access to node detail, description, comments and the node-type catalog. It continues to strip credentials and reject writes. Tests intercept those HTTP resources for repeatable CI; live SQL coverage verifies parent/type filtering, independent page caches and moderation behavior. Authentication, creation persistence, custom-type persistence, richer Markdown and production routing remain subsequent integration work.

Node page layout follows the standard ribbon card: the breadcrumb spans the card interior, with padding applied to the other sections. A left-floating importance dial shares one text flow with the short introduction and content blocks, so text returns to full width below the dial. Reactions sit after the content and immediately above the bottom actions.

Requested contribution options and selections use the catalog API’s `pluralName`, computed by the backend shared formatter with each type’s `AutoPluralize` setting. Single-node type selection retains the singular name; the browser does not invent pluralization rules.

Contribution summaries on feed and Atlas-node cards are buttons with their existing text styling plus pointer/hover feedback. Discovery supplies stable type IDs and count-aware labels for the union of requested types (including zero counts) and actual child types. Clicking opens the node page with that type selected, focuses its tab and scrolls to its feed after loading, in both Storybook and the SPA.

Child-feed navigation positions the type-tab strip 20px below the viewport top. The destination reserves enough height for empty/short feeds and corrects initial layout shifts from feed loading, images and fonts. Alignment stops on user interaction.

### Comment discussion and manual testing

The node page’s Comments tab renders live root comments in chronological pages of **10 siblings**. “Load more comments” sends both keyset cursor fields; each expanded thread has its own cursor and “Load more replies.” Replies are fetched only on expansion, never recursively preloaded. Empty/loading/error states, retry without dropping loaded comments, removed-body placeholders, author names, timestamps, and limited visual indentation are included. Feed/Atlas-node comment buttons open this tab directly.

Run `npm run storybook:api`, open a live node’s Comments tab, and use **Sign in to comment** with an existing confirmed Atlas account. Post a comment, Reply, Edit, or Remove your own comment (with an inline confirmation). Replying to removed placeholders is supported by the backend. Sign out is available in the discussion. Post more than 10 root comments or direct replies to exercise real pagination. No new seed data or migration is needed. Passwords are cleared after sign-in; comment drafts survive request failures and clear on sign-out. Backend permission checks remain authoritative.

The original `/atlas-preview-api` stays anonymous/read-only. A separate development-only `/atlas-comments-api` bridge forwards only Atlas session/CSRF cookies to the configured loopback API and allows only sign-in/out, session/token reads, single public profiles, and comment mutations. Same-origin writes and the backend CSRF cookie/header pair are required. Cookie Secure/HttpOnly/SameSite flags are preserved. Use localhost/loopback browser URLs; a static deployment must provide equivalent same-origin API routing over HTTPS. The bridge does not expose registration, account deletion, node writes or arbitrary API paths.

`tests/comments.spec.ts` supplies fixture pages and simulated sign-in for lazy loading, independent root/reply cursors, end-of-feed, author controls, post/reply/edit/removal, CSRF headers, retained drafts on failure, narrow layout, and local bridge denials. These browser fixtures do not prove real Identity cookie behavior or database persistence; manually test those against the running API. Backend HTTP/SQL tests continue to cover the real authorization and antiforgery boundary. Full suites run in CI.

The node-page tab strip uses the same backend count-aware contribution labels as the cards. It shows Comments plus the union of requested types (even at zero) and types with existing direct children; unused catalog types remain available in the creation form rather than becoming tabs. Comment counts follow the backend total, including replies and retained removed placeholders, and increment after successful posting.
