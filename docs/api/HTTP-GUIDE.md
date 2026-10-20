# Send the workflow requests

[API start](README.md) · [Authentication](AUTHENTICATION.md) · [Route index](ENDPOINTS.md) · [Blackboard guide](../blackboards/workflows/Api/README.md)

## Before you click Send Request

- Start the API with the HTTPS launch profile; check its actual listening address.
- Use an HTTP editor/client that supports `.http` requests and retains cookies.
  - Visual Studio's HTTP editor and VS Code REST clients can differ in variable and cookie configuration.
  - Enable cookie persistence in your client. A plain address-bar visit may use a different cookie jar.
- Fill variables through private editor settings or an untracked local copy.
  - Checked-in values are placeholders, not valid credentials or real IDs.
  - `csrfToken` must contain the token for the **current principal**.
  - Never commit actual credentials, cookie headers or confirmation tokens.
- Run individual requests deliberately. These files are examples for manual exploration, not an automatic setup script.

## Suggested review route

1. Open [auth.http](auth.http).
   - Optionally register and confirm a new local account.
   - Fetch anonymous CSRF → login → fetch authenticated CSRF → inspect session.
2. Open [workflows.http](workflows.http).
   - Browse the type catalog and copy an active `typeId`.
   - Create a node; copy the returned `id` into `nodeId`.
   - Read, edit, vote, comment, and inspect the resulting resources.
3. Choose another numbered workflow section.
   - Copy IDs from earlier responses when a request needs them.
   - Reuse the same authenticated cookie jar and token across files.
4. Return to `auth.http` to log out.
   - Refresh CSRF before logging in as another user or moderator.

## Find a workflow

| Number in workflows.http | Explore | Useful IDs |
|---|---|---|
| 1 | Discovery and navigation | node, optional parent |
| 2 | Node creation, types, parents and lifecycle | active type, node, parent |
| 3 | Description block composition | node, returned block |
| 4 | Ratings, reactions and undo | node, reaction association |
| 5 | Root comments, replies and cursor pages | node, comment, cursor pair |
| 6 | Communities, membership and node associations | community, node |
| 7 | Shared type and reaction catalogs | custom type, reaction definition |
| 8 | Profile browse and self-edit | participant |
| 9 | Reporting, review and moderator decisions | node; moderator session for queue/decision/history/restore |
| 10 | Notification feed and preferences | notification owned by current participant |

`auth.http` covers all six account endpoints; `workflows.http` covers the other
64 method/route pairs. [ENDPOINTS.md](ENDPOINTS.md) maps each controller action to
its request. Example coverage is not a claim that every branch is automatically tested.

## Requests are choices, not one mandatory chain

- An existing confirmed account can skip registration and confirmation.
- Archive/restore, membership join/leave, and vote/undo are optional cycles.
- Node restore reverses the author's archive. Moderator restore is a separate workflow.
- A decision to dismiss a report does not require author editing or restoration.
- Editing/removing blocks or comments requires a suitable owned resource.
- Catalog changes require a user-created definition when system definitions prohibit mutation.
- Moderator requests require an existing persisted GlobalModerator role; JSON cannot grant it.

## Inspect failures as well as successes

- **400:** invalid request, invalid cursor pair, or CSRF failure; inspect the safe JSON problem.
- **401:** no valid session for a protected endpoint.
- **403:** ownership or role rejection.
- **404:** resource missing or a read intentionally hiding original moderated content.
- **409:** the domain rejects the operation in its current state.
- **429:** a domain quota or host limiter rejects work.
  - Domain quota responses include a safe message and `Retry-After`.
  - Do not assume the host's concurrency/source limiter supplies the same body/header.
- **503:** registration is unavailable outside Development until production email delivery is configured.

Comment pages return `next.afterAt` and `next.afterId`. Supply both together,
keep the same node/parent scope, and stop when `next` is null. Expanding replies
uses `parent=commentId`; it does not load the entire descendant tree.

## Manual Identity requests

- [auth.http](auth.http): register, confirm, login, session and logout.
- [identity-recovery.http](identity-recovery.http): reset/resend workflows and consumed-token replay.
- [identity-negative.http](identity-negative.http): CSRF, credentials, token and validation failures.

Run requests individually with cookies enabled. Copy the returned CSRF token after every
login/logout into the file you are using; keep credentials and private mail tokens untracked.
These are manual checks with expected outcomes, not automatically verified test results.
