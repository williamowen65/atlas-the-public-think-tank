# Agent instructions for Atlas

These repository instructions apply to AI coding agents and other automated contributors. Prefer small, reviewable changes, preserve the existing security boundaries, and verify changed code before proposing a merge.

## High-risk file: `src/hosts/Atlas.Web/dev/localApi.ts`

This file defines the Vite/Storybook development API guards, same-origin validation, and explicit allowed routes. **Do not edit it without validating TypeScript parsing afterward.** A malformed regex or template literal here prevents both the web dev server and Storybook from starting.

### Recurring failure and its precise cause (2026-11-09)

While adding content-block move/remove routes, a programmatic `String.replace(search, replacementString)` edit accidentally inserted much of `localApi.ts` into itself. The replacement string contained the two-character sequence **dollar sign followed immediately by a backtick** (the end-of-regex anchor `$` followed by a closing template-literal delimiter). In a JavaScript replacement *string*, that sequence is a special replacement token meaning **the text before the match**. It is *not* treated literally. This can duplicate the file, corrupt regular-expression declarations, and trigger an apparently unrelated parsing error such as `Expected ")" but found "Received"` on the otherwise-valid `Received Host:` line.

**Never use a plain replacement string for code fragments that might contain dollar signs.** Instead use the replacement callback, which inserts returned text literally:

```js
const result = source.replace(anchor, () => replacement);
```

For a simple one-off edit, an exact manual patch with narrow context is preferable. Verify the target anchor occurs exactly once, compare changes against the base branch, and watch for unexpectedly large file-size/diff changes or repeated imports/declarations. Do not make broad line-oriented substitutions into long template-literal/RegExp allowlists.

### Mandatory checks before any commit involving this file

From `src/hosts/Atlas.Web`:

```sh
npm run check:dev-proxy
npm run check
```

The first command (`dev/checkLocalApi.mjs`) uses the TypeScript parser and checks unique declarations, precisely to catch malformed template literals and accidental duplication. The full check also validates the Svelte/TypeScript app. Do not claim tests passed if they were not executed; keep changes as an unvalidated draft instead.

Additional focused check, if needed:

```sh
npx esbuild dev/localApi.ts --bundle --platform=node --packages=external --outfile=localApi-check.js
```

Delete the temporary build output after this command. Review `git diff --check` and `git diff -- src/hosts/Atlas.Web/dev/localApi.ts` before pushing. Keep the proxy method/path allowlist synchronized with intended backend endpoints, but do not weaken the loopback/LAN restrictions, same-origin checks, cookie forwarding policy, or backend CSRF/authorization.

Any new development proxy route must be intentionally minimal in method and path; tests must cover allowed and rejected methods/origins. The proxy is not an authorization boundary and does not replace the server's checks.
