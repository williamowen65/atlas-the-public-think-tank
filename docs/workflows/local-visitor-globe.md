# Account pin globe (PTT-162 / PTT-163)

The circular desktop globe and mobile bottom control open a compact popup using Atlas's existing theme. Outside clicks dismiss it without blocking header navigation. Escape, Close and the launcher also dismiss it. The backdrop covers the viewport through scrolling/resizing. The miniature and expanded globes share one rotation clock; reduced-motion preferences stop rotation.

## Which pins appear

- **Historical:** accounts that chose to save a location pin. Accounts without a pin never enter this map or its totals.
- **Live:** those pinned accounts with activity in the last **five minutes**. Each account counts once across tabs and devices.
- **Anonymous viewers:** can view the map, but cannot add a pin or activity record and are not counted. Old anonymous pin records are excluded from both views; they are not converted into account pins.
- **Remove my shared location:** removes the account's pin from both views and totals. Later activity does not recreate it.

The UI labels Live as recently active, rather than claiming instantaneous connection status. After closing the site, signing out, or leaving all tabs hidden, an account leaves Live when its five-minute activity window expires and the next snapshot refreshes. Its saved pin remains in Historical.

## Refresh and API cost

There is **no real-time listener, EventSource, SSE endpoint, WebSocket, broadcast loop, or connection registry**. The eight-per-visitor and 32-total stream limits are removed.

The browser loads a short JSON snapshot on page load and navigation, when opening the globe, and about every **60 seconds while visible**. Hidden pages pause polling and refresh on returning. It never overlaps its own polls. A failure retains the saved snapshot internally but labels the current update unavailable and offers Retry; the next regular poll can recover.

Anonymous/unpinned views use a GET. Pinned, signed-in accounts use a CSRF-protected activity POST that updates their existing pin's last-active time and returns a snapshot. Initial loading learns the account's pin state before posting activity. Multiple tabs' database updates are coalesced to at most once per 45 seconds per account.

The API shares public snapshot data for up to ten seconds to reduce database reads; personalized sign-in/pin state is computed separately. Normal requests end after their JSON response, so they do not hold API slots for the lifetime of a browser tab. There is no globe visitor/connection cap preventing 100 or more saved/recently-active accounts from appearing. Ordinary API capacity and database costs still apply; this is not an Azure load-test result or an unlimited-capacity claim.

## Sign-in and location consent

Pin/activity writes require the API's confirmed-member policy and CSRF validation. The server derives the account key from the verified request actor, never an account ID submitted by the client. No anonymous globe cookie is issued or forwarded.

Signed-out viewers see **Sign in to place a pin**. Signed-in viewers can choose **Share my location on this globe**; a globe-specific explanation precedes the browser's permission prompt. The consent panel includes a labeled color picker and a preview of the chosen pin. The selected six-digit hex color is saved with the pin and returned in both snapshots. Existing pins without a color use red (`#b73539`). Only whole-degree coordinates leave the browser; the API validates and rounds again. Approximate pins aggregate when both their coordinates and selected colors coincide, and account names/IDs are not returned in map data. Existing saved pins show a removal action instead of another share button.

## Required local migration

The developer-generated `20261108173914_AddGlobeVisitorHistory` migration remains unchanged. This redesign adds nullable `GlobeVisitors.LastSeenAt` and removes the unused `GlobeBrowsers` model/table. The existing account pin keys remain compatible. Existing anonymous visitor rows remain excluded by query; no migration should copy them into accounts.

The developer-generated `20261108210201_AddAccountPinActivity` migration, designer and snapshot are now committed on this branch. Do not generate a second migration with the same purpose. `src/Atlas.Persistence/AGENTS.md` reserves migration generation and application for the developer. From the solution root, with shared user secrets or `ATLAS_SQL_CONNECTION_STRING` targeting **local SQL Server**, apply and verify the committed migration:

```sh
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlGlobeHistoryTests
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --filter FullyQualifiedName~Globe
```

SQL durability/translation and the full database workflow remain unverified until this developer step is complete. Do not target Azure during the capstone review.

## Run and verify locally

```sh
cd src/hosts/Atlas.Web
npm ci
npm run dev:api
```

Use the existing local HTTPS/LAN setup for secure session and CSRF cookies. Verify:

1. Signed out: snapshots load, neither view counts you, and pin placement links to sign-in.
2. Signed in without a pin: still not counted. Declining location consent creates no record.
3. Save a pin: the same account appears once in both views, including across browsers/tabs.
4. Leave one visible tab open: activity refreshes about once per minute through short requests.
5. Close/hide all tabs: after five minutes, refresh another browser's map. The pin remains in Historical and has left Live.
6. Return: the next activity refresh brings the existing pin back into Live.
7. Remove the pin: it disappears from both views and totals. Polling does not recreate it.
8. Developer tools should show snapshot/activity JSON requests, with no event stream or WebSocket connection.

Focused UI checks use fixture snapshots, not the database:

```sh
npm run build
npx playwright test --config playwright.globe.config.ts
```

Service tests cover 120 account pins, uniqueness, expiry, cache isolation and removal. HTTP tests cover finite anonymous reads, denied anonymous writes, and removed listener routes. SQL tests cover persistence and exclusion of legacy anonymous rows after the committed migration is applied locally.

## Storybook appearance

Run `npm run storybook`, then open **Components → VisitorGlobe → Playground**. Controls cover ocean center/edge colors, land, grid, outline and pin colors, size, fixture pins, rotation speed/angle and animation. Set both ocean colors equal for a flat fill. Use pinCount (0–10,000) and distribution (worldwide, clustered, custom) to explore fictional demo data. Presets include Empty, SparseWorldwide (12), BusyWorldwide (100), DenseWorldwide (500), Clustered (150), and CustomPins. Custom mode uses the editable pins array. Positions are deterministic so style comparisons keep the same map data. Paper, Night and Miniature provide alternate starting points. These renderer-only stories need no API, account, database or geolocation permission; their controls do not change site preferences.

The rotating orthographic globe adapts William Owen's [GeographicPinMap](https://github.com/williamowen65/GeographicPinMap) at `68febc15e1b9f0509445339744f9e7c07def16c2`; its original Socket.IO listener is not used. Natural Earth land data is vendored locally and public domain; credit is optional. The information panel and `static/globe/README.md` retain its source link.

## Demo before merge

Keep this PR unmerged and work locally while the capstone deployment review is pending. After that review and explicit demo deployment authorization, apply reviewed migrations and deploy this branch to **demo only**. Verify sign-in enforcement, periodic refresh, recent-activity expiry, history persistence, pin removal, and ordinary app responsiveness before merging. Keep live unchanged. No Azure resources or deployment workflows are changed by this implementation.

Pin controls include round or flat heads and stems only, a single head color or varied colors, shaft color, head radius, shaft length and lean. FlatHeads, StemsOnly and EquatorAndHorizon provide focused comparisons. Lean is deterministic for each location, so pins do not wobble when snapshots refresh. The shafts and heads use the same orthographic camera as the map; far-side parts render behind the globe, allowing raised tips to appear over the horizon without showing through the Earth.

### WebGL rendering and performance

The application and Storybook use one WebGL 2 renderer. SVG and Canvas 2D globe implementations and their comparison controls have been removed. D3 still prepares the local map texture. Stationary pin geometry is uploaded to GPU buffers when data or settings change; rotation updates camera uniforms. Depth testing hides far-side pins while retaining raised tips over the horizon.

Use **targetFps** (10, 15, 24, 30 or 60) and **pinCount** (0–10,000) to explore loads. Stress starts with 10,000 pins. The application targets 60 animation updates per second. The expanded globe mounts on open and releases its GPU resources on close; both views keep the same shared angle. The displayed rotation updates/s measures delivered updates, not completed GPU paints. Hidden tabs and reduced-motion preferences stop rotation. API polling is independent of drawing frequency.

For repeatable tests keep globe size, distribution, styles, speed, browser zoom and viewport fixed. Compare 100, 1,000, 5,000 and 10,000 pins, then test the miniature separately. Device, browser and pixel density affect results. The automated tests use software WebGL and do not establish hardware performance limits.

If WebGL 2 is unavailable, the expanded view explains the condition and retains the existing textual account counts and location lists. A context-loss message remains until restoration rebuilds GPU resources. The compact launcher keeps a globe symbol when graphics are unavailable. There is no alternative drawing backend.

See [Learning WebGL through the Atlas globe](../architecture/visitor-globe-webgl.md) for the shader, buffer, instancing, camera and depth-testing walkthrough.


### Local migration for pin colors

The model now adds nullable `GlobeVisitors.PinColor` (maximum length 7). Existing rows remain nullable and are displayed with the default red; no account identities are added to snapshot data. Per `src/Atlas.Persistence/AGENTS.md`, the developer must generate, review and commit the migration/designer/snapshot. No migration has been generated or applied for this column, and SQL persistence is not verified until the local migration and SQL tests run.

From the solution root, with the connection targeting **local SQL Server**:

```sh
dotnet tool restore
dotnet ef migrations add AddGlobePinColor --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlGlobeHistoryTests
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --filter FullyQualifiedName~Globe
```

Worldwide Storybook fixtures now use region-weighted land-only positions, checked against the local Natural Earth coastline after whole-degree rounding. They illustrate usage rather than model population. Uniform remains an ocean-inclusive sphere stress test; Clustered retains tight city anchors. Demo colors are seeded independently of position and lean, preventing repeated color bands in clusters. Real account coordinates remain rounded to whole degrees for privacy.


### Choose a location without browser permission

Signed-in accounts can open the globe, choose **Share my location on this globe**, then **Choose a spot on the globe instead**. This path never calls browser geolocation. Rotation pauses for both globe views. Use **Turn globe** to reach another region, then click/tap Earth to place a draft pin. The coordinate fields provide an equivalent keyboard path. Coordinates must be valid whole degrees before Save is enabled. The preview uses the selected color; account totals and saved data do not change until **Save this pin** succeeds. Cancel or closing the panel discards the draft.

Existing pinned accounts can choose **Edit my pin**. Their saved color is loaded through the account-specific snapshot state, while public snapshots never include that personal field. Picking and saving replaces the existing pin rather than adding a second pin. The API and database continue to store one approximate pin per account, not travel history. Browser-assisted placement remains available as a separate option. Real users may choose ocean locations; only the illustrative land-based fixture distribution excludes oceans.

Manual picking inverts the same orthographic camera as the shader: screen coordinates map to a sphere point, the camera rotation is reversed, then latitude/longitude are rounded. Outside-Earth clicks do not place a pin. Editing mode uses a smaller globe and a bounded scrollable panel to keep its controls reachable on desktop and mobile. No additional schema change is needed beyond the pending AddGlobePinColor migration.
