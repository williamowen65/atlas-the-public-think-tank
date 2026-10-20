# API workflow blackboard

Open [ApiWorkflows.excalidraw](ApiWorkflows.excalidraw) in native Excalidraw (or an
Excalidraw editor extension). Zoom to one numbered region at a time. Card text
links to the implementing code on the PR branch.

[API starting guide](../../../api/README.md) · [Authentication](../../../api/AUTHENTICATION.md) · [Send requests](../../../api/HTTP-GUIDE.md) · [All endpoints](../../../api/ENDPOINTS.md)

## Choose a reading path

| Region | Question answered | Request companion |
|---|---|---|
| 01 | When do I obtain/refresh CSRF, get a session, and remove it? | `auth.http` |
| 02 | What attack does CSRF prevent, and why protect every write? | Authentication guide; missing/invalid-token tests |
| 03 | In what order do middleware, policies, filters and the action run? | `Program.cs`, `ApiBoundary.cs` |
| 04 | How does an author discover, create and maintain a node? | workflows sections 1–2 |
| 05 | How do blocks, parent links and types change? | workflows sections 2–3 |
| 06 | How do ratings, reaction votes and paged comment replies work? | workflows sections 4–5 |
| 07 | How do communities differ from shared type/reaction catalogs? | workflows sections 6–7 |
| 08 | Which actor reports, reviews, hides or restores content? | workflows section 9 |
| 09 | How do profile edits and recipient notification actions work? | workflows sections 8 and 10 |
| 10 | Which code runs at startup, per request, or in the background? | Code guide and API tests |
| 11 | How does a brand-new user register, confirm email, and first sign in? | `auth.http` A1–A6 |
| 12 | Which signup checks reject work, and what happens if local delivery fails? | AccountsController / AtlasAccounts |

## Follow a new user

Start at **region 11**, then use **region 12** to inspect the failure boundaries.

1. Fetch anonymous CSRF, then submit registration data.
2. The host requires Development mode and a configured private mail-drop path.
3. `AtlasAccounts` creates the Identity user, Participant profile and Member role link in one SQL transaction.
   - The shared participant/account ID and role come from the server.
   - Email remains unconfirmed; no authenticated session is created.
4. After commit, the host generates a confirmation token, writes the private JSON file, and returns 202.
5. Copy that file's `userId` and `token` into the confirmation request.
   - Send the anonymous CSRF pair too; these are **two different tokens**.
6. Confirmation returns 204 and marks the email confirmed; it does not sign in.
7. Login returns 204 with the session cookie. Fetch a fresh CSRF token for the signed-in principal, then inspect `/api/auth/session`.

Local confirmation-file delivery is not part of the SQL transaction. If that file
write fails after commit, the API can return 500 while the unconfirmed account
already exists. Do not infer rollback from that response or expect a resend endpoint;
production delivery/recovery is still separate work.

## What the arrows mean

- A chain gives a useful review sequence, not a requirement to call every endpoint.
- Independent vertical lanes represent separate optional cycles.
- Region 02's decision branches show token acceptance versus rejection.
- Region 08 follows **HideNode**; **Dismiss** ends the report branch without restoration.
- Abbreviated route fragments in a region share the base stated in its note.
- Blue is application flow; orange is a gate/rule; green is success; cyan is a read;
  purple is lifecycle/storage; red is rejection; gray dashed is future work.

## Keep these distinctions in mind

- **Session cookie:** establishes identity; the client cookie jar honors login/logout responses.
- **CSRF pair:** guards unsafe requests; refresh the JSON/header token when principal changes.
  - Reuse it for subsequent writes by the same principal.
  - The confirmation token in a development email payload is a different kind of token.
- **Authorization:** role and resource ownership checks still apply after CSRF succeeds.
- **Author restore:** reverses author archive; **moderator restore** follows moderation prerequisites.
- **Shared DB:** API and Console target the same configured database and domain rules.
- **HTTP examples:** cover all 70 route/method pairs, but are not automated proof of every branch.
- **Boundaries:** each request can have its own operation/transaction; the board does not imply one transaction across a user journey.

The API remains JSON-only. A future SPA, CORS deployment, production mail provider,
shared encrypted Data Protection keys and 2FA remain separate work.

## Source and test entry points

- [Program / pipeline](../../../../src/hosts/Atlas.Api/Program.cs)
- [API service registrations](../../../../src/hosts/Atlas.Api/ApiServices.cs)
- [Boundary middleware / errors](../../../../src/hosts/Atlas.Api/ApiBoundary.cs)
- [Controllers](../../../../src/hosts/Atlas.Api/Controllers)
- [Boundary tests](../../../../tests/Atlas.Api.Tests/BoundaryTests.cs)
- [SQL workflow journey](../../../../tests/Atlas.Api.Tests/SqlWorkflowTests.cs)
