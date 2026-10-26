# Playwright consistency investigation

Tracking: [PTT-152](https://thepublicthinktank.atlassian.net/browse/PTT-152).
[Evidence notebook](https://thepublicthinktank.atlassian.net/wiki/spaces/PTT/pages/8355841).

## Scope

Two separate observations: unexplained web-server proxy connection failures and
one failing corner-action layout assertion. Do not infer a shared cause.
Branch baseline: `c4bec692d1fa56be728ba11d1d16b9e0a25da3df`.
User's exact local run SHA and browser version still need recording.

## Proxy errors

Reported: HTTP proxy errors for /api/auth/session, connection refused, while
many tests pass. Earlier inspected CI frontend logs at d8c7817 also recorded
three such errors alongside 100 passing tests.

The main Playwright configuration starts Storybook development on 6007, Vite
preview on 4173 and static Storybook preview on 6006. It does not start Atlas.Api
or the Ranking Lab API; browser tests are intended to isolate those dependencies.

Investigate requests that escape mocks, initial background session loading,
service workers, startup/shutdown requests and deliberate failure cases.
A web-server log's position next to a test is not proof of attribution,
especially with parallel workers.

For each error identify the full request path, originating page/story, test
title/file, retry/worker and lifecycle phase. Where practical attach browser
request-failure evidence to that test, and determine whether the proxy rewrites
the path before logging it. Browser HTTP 4xx/5xx responses and network failures
are different events and should be recorded separately.

If a case intentionally tests failure, explicitly document and assert that
expected failure in the named test. If an unrelated background request is
unmocked, provide the appropriate deterministic fixture. Do not merely suppress
all proxy errors or start a real backend to conceal isolation gaps.

## Corner action layout failure

User reports:
- tests/live-preview.spec.ts:409, “corner action floats at the description end above the navigation counts”.
- Poll at line 426 expected absolute description/action bottom difference < 1 px;
  actual difference 26.390625 px after 5 seconds.
- Test iterates short/long descriptions and widths 1024, 640 and 320.
  The supplied output does not identify which iteration failed.
- Existing artifacts: error-context.md and trace.zip under
  test-results/live-preview-corner-action-c0256-above-the-navigation-counts/.

The inspected test uses static Storybook on port 6006 and mocks public API reads.
It refreshes after changing description, then resizes and polls geometry.
Inspect trace/screenshot, exact iteration, font readiness, refresh completion,
computed float/layout and actual browser version before changing CSS or assertions.
Do not widen tolerance or add retries to mask a genuine layout problem.
CI passing for this exact layout test at the current SHA has not yet been checked.

## Reproduction and evidence

From src/hosts/Atlas.Web, capture SHA, clean/modified status, Node/npm,
Playwright and launched browser versions, OS and command. Build before testing:

```powershell
npm run build
npm run build-storybook
npx playwright test tests/live-preview.spec.ts -g "corner action floats" --workers=1 --trace=on
```

Compare isolated and full-suite behavior, retaining the original failure trace.
Use named test steps or assertion messages for viewport/description cases.
Keep a record of expected versus unexpected errors and verified Windows/CI
results at matching SHAs.

## Status

Initial source review and user failure captured. No executable changes yet.
New local/CI reproduction and root-cause analysis remain open.
