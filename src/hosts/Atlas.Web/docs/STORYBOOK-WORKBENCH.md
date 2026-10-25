# Atlas component and style workbench

Storybook is a development/review surface for the same Svelte components used by the SvelteKit application. It does not own application routes, domain rules or a second frontend implementation. Start with [application wiring](../../../../docs/architecture/diagrams/WIRING.md) for presentation and backend ownership, or the [frontend README](../README.md) for environment setup and scripts.

## Choose a surface

| Sidebar area | What it exercises |
|---|---|
| Styles and Layout | Shared typography, spacing and color experiments |
| Layout | Header and footer components |
| Components | Reusable controls such as VotingDial, Breadcrumb, sign-in and the node form |
| Compositions | Live feed, node page, profile, node preview, voting comparison and search diagnostics |

Story metadata supplies sidebar paths; story wrappers adapt navigation and inputs. Do not copy their API/navigation setup into production components. Component Controls vary inputs; live compositions can read and mutate the actual local API. Isolated examples and browser tests can use fixtures or intercepted responses, so they are not proof of live authentication or persisted writes.

## Start the appropriate services

Run these from `src/hosts/Atlas.Web` after `npm ci`:

| Command | Starts / requires |
|---|---|
| `npm run storybook` | Storybook only; start the API separately for live API stories |
| `npm run storybook:api` | API and Storybook together |
| `npm run dev:api` | API and SvelteKit development server together |
| `npm run storybook:ranking` | Local ranking diagnostics host and Storybook; other live stories still need the API |

The API and ranking host can run together. Choose one launcher for the Storybook server rather than starting two servers on its port. [Ranking diagnostics](../../../../docs/discovery/search-ranking-workbench.md) use the actual local `ATLAS_SQL_CONNECTION_STRING`, not disposable fixture databases. Do not use the SQL test connection as the workbench corpus.

The shared [development bridge](../dev/localApi.ts) provides separate allowlisted public/session routes in Vite and Storybook development. The session bridge enables existing-account sign-in, CSRF/session reads and selected mutations; backend authorization and antiforgery checks remain authoritative. Static app/Storybook builds need equivalent hosting routes. No development proxy is automatically deployed with static output.

## Shared styles and controls

`src/styles.scss` is imported by the app layout and `.storybook/preview.ts`. Theme and Density toolbar values set document-level tokens across stories. Typography/design controls use the shared design settings and reset behavior; individual component args remain separate. Refresh can preserve Storybook args, so use the story/reset controls to return to defaults. These experiments change preview state rather than source code; they do not themselves add a production theme-preference screen.

## Follow a live node

Open **Compositions / Feed** or **Compositions / Node page**. Read more and contribution/comment actions navigate through Storybook adapters to the corresponding live node composition. Node content, comments/replies, creation and voting use reusable components and API clients. Signed-in ownership determines whether node editing is offered. See [node editing and private revision drafts](NODE-EDITING.md) and [the component catalog](COMPONENT-CATALOG.md).

`VotingDial` is caller-controlled presentation; `LiveVotingDial` supplies server summaries, confirmation/undo, sign-in prompts and failure recovery. It does not calculate authoritative community averages in the browser. The standalone voting comparison can remain an isolated UI example without claiming that its local values were persisted.

## Verification

Use `npm run check`, `npm run build`, `npm run build-storybook` and the relevant Playwright tests. `npm run test:ci` combines the frontend build/test workflow. Browser fixtures exercise UI contracts; SQL tests exercise persistence/search; a running-API smoke test checks real cookies, authorization and saved data. Keep those kinds of evidence distinct.

For source-reading help, use [Svelte syntax](SVELTE-READING-GUIDE.md) and [SvelteKit entry points](SVELTEKIT.md). Historical frontend review notes remain [pinned to PR #360](../../../../docs/architecture/ATLAS-WEB-PR-360-REVIEW.md); current startup and ownership guidance lives here and in the linked owning guides.
