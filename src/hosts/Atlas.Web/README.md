# Atlas.Web component workbench

PTT-143, PTT-144, PTT-145 implement the starting point from [PTTD-31](https://thepublicthinktank.atlassian.net/browse/PTTD-31). The priority is isolated component development and automated browser checks. There is no selected SPA framework, router, state library, API integration, or production page design yet.

## Start here

Install Node.js 24 LTS, then run these commands from this directory:

```sh
npm ci
npm run storybook
```

Open http://127.0.0.1:6006. Use the Foundation / ActionButton stories and Controls panel to change the label and disabled state. A story is a named setup condition for a component. `args` supplies its inputs; the render fixture supplies a callback and visible result. These fixtures require no account, database, or running API.

In another terminal, `npm run dev` opens the minimal integration harness at http://127.0.0.1:5173. Both surfaces import `src/components/ActionButton.ts`; editing that module updates both. This button proves the tooling, rather than prescribing Atlas's eventual design.

## Source structure and the deferred SPA choice

| Location | Responsibility |
| --- | --- |
| `../Atlas.Api` | Existing C# HTTP host and Identity |
| `Atlas.Web/package.json` | Independently runnable frontend workspace |
| `.storybook/` | Storybook renderer, addon, and preview configuration |
| `src/components/` | Shared rendering code and colocated story scenarios |
| `src/main.ts` | Tiny web composition proving reuse |
| `tests/` | Browser checks against built stories and web composition |

Atlas.Api is a sibling of Atlas.Web, not nested within it. Storybook is a separate command and browser surface within the same npm workspace; it is not a C# project or a deployed application dependency. Atlas.sln exposes the workspace files as solution items for discoverability. npm commands still build and test them independently of dotnet.

The interim renderer is Storybook HTML with Vite and TypeScript. Components currently return native DOM elements. This deliberately avoids selecting React, Vue, Svelte, a custom-element architecture, or an application state model. Choosing a framework later will require revisiting the renderer and possibly adapting these small render functions; this is not a promise of zero migration cost. Keep the initial sample small until that decision is researched.

Redux remains a candidate for application state, not a prerequisite for Storybook or a choice made here. Consider its need alongside the eventual framework and real shared-state requirements. Storybook itself may contain React dependencies for its developer interface; that does not make Atlas a React application. React's [current MIT license](https://github.com/react/react/blob/main/LICENSE) contains no restriction on competing with Facebook.

For the MVP, the working assumption is one public origin: `/` for the SPA and `/api` for the API. Source layout does not decide hosting. A later task can have ASP.NET serve built assets or route one public origin to separate processes. This harness makes no API requests and does not weaken HTTPS, cookies, CORS, or CSRF. PTT-147 covers that integration.

## Run the CI checks locally

```sh
npx playwright install chromium
npm run test:ci
```

On a Linux machine missing browser libraries, use `npx playwright install --with-deps chromium`. `npm run build` type-checks and builds the web harness. `npm run build-storybook` produces local static workbench files. `npm test` starts temporary servers for those built outputs, runs Chromium checks, and stops the servers. Build again after changing source; tests intentionally target built artifacts.

The checks cover enabled keyboard activation, rejected disabled activation, narrow-screen wrapping, accessibility rules for the enabled story, and shared-component behavior in the web harness. Accessibility automation cannot establish complete accessibility: still review keyboard navigation, assistive technology, contrast, and actual product workflows.

Storybook provides isolated scenarios; Playwright supplies real browser interaction and assertions. This overlaps browser automation tools such as Selenium, but these component checks do not replace later full-app journeys for registration, sessions, navigation, and real API behavior. Direct Playwright checks are used here instead of the React/Vue-oriented Vitest addon, allowing the HTML renderer to remain independent of a SPA choice.

GitHub Actions installs locked dependencies and Chromium, builds both surfaces, and runs the same checks without SQL, secrets, a public Storybook URL, or a paid visual service. Failed runs retain an HTML report and browser traces for seven days. Open reports with `npx playwright show-report`; inspect a trace with `npx playwright show-trace <trace.zip>`. No Storybook deployment is configured. Storybook telemetry is disabled in both commands and configuration.

## Next decisions

Research the SPA framework before expanding the component library (PTT-146). Evaluate component ergonomics, routing, state needs, testing, maintenance, licensing, and eventual SEO/share previews. Framework selection and deployment remain separate decisions. The Storybook workflow can be reviewed now with only a small web composition.
