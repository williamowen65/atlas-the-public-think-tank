# Visitor globe (PTT-162 / PTT-163)

The circular desktop corner globe opens a compact visitor map centered over the page body. The Atlas header stays visible; on phones, the popup also stays above the bottom navigation. It uses the site's existing surface, text, border, radius and accent tokens. The popup is a non-modal dialog: outside clicks dismiss it, and the same click can activate a header link or control. Escape, the close button, or its launcher also dismisses it. Escape and the close button return focus to the launcher. A pointer-transparent backdrop covers the full viewport, including while scrolling and resizing; it does not make the surrounding site inert. Live / Historical controls switch counts and pins together.

The miniature and expanded globe share one rotation clock, so opening or closing the popup keeps the same part of the world in view. Reduced-motion preferences pause rotation. Pins use thin outward-projecting stems rather than circular dots. The globe has a centered ocean tint without a diagonal lighting band.

Visitor totals, connection status and the current location-sharing action stay compact beneath the globe. Saved sharing state is read from the visit response, so an already-shared location offers removal instead of another share button. Connection status describes the listener, separately from location consent. Counts, text locations and the optional Natural Earth credit are behind **About this globe**.

The globe is intended to run in both local development and the deployed Atlas application. The frontend includes it in production builds, and Atlas.Api serves its endpoints in every environment. The Vite proxy only supports local development; Atlas.Api owns the deployed routes, not Sites.

**Current rollout plan:** develop and test this branch locally while the capstone deployment review is pending. Leave the PR unmerged and do not deploy or apply migrations to Azure during that review. Once the review is complete, explicitly deploy this branch to the demo app, apply its reviewed migration to the demo database, and verify the hosted experience before merging. Deployment timing is a workflow decision, not an environment restriction in the feature code.

## What the counts mean

- **Live:** distinct visitor identities with at least one open globe event stream anywhere in the app. Multiple tabs count once. Closing the last connection removes live presence.
- **Historical:** distinct recorded visitor identities in this database, including people who never share a location. It survives API restarts. The displayed date is the earliest retained visitor record, not a rollout date or an analytics reporting window.
- Confirmed signed-in accounts use the verified server-side participant identity, hashed before storage. Anonymous visitors use a protected, HttpOnly, SameSite browser cookie. A sign-in upgrades anonymous history instead of counting it twice; signing in to the same account in another browser also counts once.
- This cannot identify unique human beings perfectly: cookie clearing, private browsing, unlinked devices and multiple accounts can affect totals. It does not use IP addresses or fingerprinting. Anonymous-to-account upgrades discard the anonymous pin; they do not silently transfer location consent.
- Historical pins represent each visitor's **latest voluntarily shared approximate location**, not a trail of past locations. Removing a pin clears it from both views but retains the visitor count.

## Location sharing

Nothing asks for geolocation on page load, opening the globe, or switching views. The visitor first selects **Share my location on this globe**, sees an explanation tied to the Atlas globe, and then selects **Continue: share on this globe** before the browser permission prompt opens.

Exact coordinates stay in the browser. It sends whole-degree longitude/latitude; the API also validates ranges and rounds again. Pins are anonymous, aggregated when positions coincide, and listed in text below the globe. Whole-degree locations can be tens of miles from the actual position, especially away from the poles. A **Remove my shared location** action clears persistent coordinates.

The normal browser permission prompt uses the site's origin; its browser-owned wording cannot be customized. The preceding Atlas explanation gives that prompt its globe context. Permission denial, unsupported geolocation and save failures leave the globe usable without a pin.

## Local migration

`src/Atlas.Persistence/AGENTS.md` reserves migration generation and application for the developer. The developer-generated `20261108173914_AddGlobeVisitorHistory` migration, designer and snapshot are now included. Do not generate a duplicate migration for this change.

The generation command was `dotnet ef migrations add AddGlobeVisitorHistory --project src/Atlas.Persistence`. Review the committed migration, which adds `GlobeVisitors` and `GlobeBrowsers` with their foreign key. From the solution root, with `ATLAS_SQL_CONNECTION_STRING` or shared user secrets targeting your **local SQL Server**, verify and apply it yourself:

```sh
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlGlobeHistoryTests
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --filter FullyQualifiedName~Globe
```

During the capstone review, use only local connection strings and do not run Azure setup/deployment workflows. SQL persistence and the full local database experience remain unverified in this workspace; committing the migration does not by itself verify application to a database.

## Run and verify

Start the usual local API and web development workflow:

```sh
cd src/hosts/Atlas.Web
npm ci
npm run dev:api
```

Use the existing HTTPS local setup so the session/CSRF/visitor cookies retain their secure attributes. The development proxy forwards only the allowlisted globe operations and the Atlas cookies; the API still validates CSRF on writes.

1. Open Atlas in two tabs in the same browser: Live should be **1**.
2. Open an independent browser profile/private context: Live should become **2**. Two windows sharing the same browser cookies are still one visitor.
3. Choose to share a location in one session: both sessions receive the approximate pin without reloading.
4. Switch to Historical: counts and retained pins reflect the local database.
5. Close one of the first browser's tabs: the visitor stays live. Close its last tab: Live drops, Historical remains.
6. Remove your shared location: the pin disappears in both views and in the other connected browser.
7. Stop/restart the API: the globe reconnects automatically. Historical records persist; Live rebuilds from current connections.

Focused browser checks (mocked event data; no database required):

```sh
npm run check
npx playwright install chromium
npm run build
npx playwright test --config playwright.globe.config.ts
```

Both Playwright configurations exercise the globe in the production frontend build served locally. API tests separately boot real Development and Production hosts and exercise SSE responses through the session API prefix with independent cookie jars and an in-memory history adapter; these do not establish SQL durability or verify Azure. SQL persistence tests exercise merging, restart/repository reload, pin aggregation and removal after the migration is generated.

## Explore the globe in Storybook

Run `npm run storybook` from `src/hosts/Atlas.Web`, then open **Components → VisitorGlobe → Playground**. The Controls panel exposes ocean center/edge colors, land, grid, outline and pin colors, grid visibility, globe size, fixture pins, rotation speed, angle and animation. Give both ocean colors the same value for a flat fill.

**Paper**, **Night**, and **Miniature** stories provide alternate starting points. These stories use the same renderer as the site, with fixture pins and a vendored map; they need no API, database or geolocation permission. Changes in Storybook are exploratory controls, not saved site preferences. Rotation controls are only in Storybook.

Natural Earth data is public domain and its terms explicitly make credit optional. The reference GeographicPinMap loaded the D3 Graph Gallery's `world.geojson`, rather than this vendored Natural Earth land file. Keep source/licensing notes in `static/globe/README.md`; the popup credit is informational.

## Demo verification before merge

After the capstone deployment review is complete and demo deployment is explicitly authorized:

1. Review the committed migration and verify the local database round trip first.
2. Deploy this branch and apply the reviewed migration to **demo only**, using the existing workflows. Keep live unchanged.
3. Verify the miniature globe in the hosted production build on desktop and mobile.
4. Repeat the two-browser checks above, including SSE updates without reloads, optional location sharing over HTTPS, pin removal, reconnection, and Historical persistence after an API restart.
5. Confirm the actual Azure proxy delivers SSE updates/heartbeats promptly rather than buffering them, and ordinary API requests remain usable while listeners are connected.
6. Review demo results before merging. A local build or a test host configured as Production does not establish that the Azure path works.

## Implementation and hosting limits

The reference is William Owen's [GeographicPinMap](https://github.com/williamowen65/GeographicPinMap) at `68febc15e1b9f0509445339744f9e7c07def16c2`. Its D3 orthographic projection, rotating globe and explicit pin interaction are adapted into Svelte components. The original Express/Socket.IO server is replaced with a .NET server-sent event listener; the feature requires only server-to-browser updates, while normal CSRF-protected HTTP writes handle visits and location changes.

The map asset is vendored Natural Earth public-domain land geometry; no runtime CDN, Glitch endpoint or external map request is used. D3 geometry is bundled through npm. See `src/hosts/Atlas.Web/static/globe/README.md` for attribution.

Live presence is **single API process** and capped at 32 streams total / 8 per visitor, leaving half the existing 64-slot API concurrency budget for other requests. Each subscriber has a bounded, latest-snapshot queue; disconnects and cancellations clean up listeners. Reconnection is automatic and unavailable live data is not presented as current. Verify the single-process assumption and capacity during demo testing. Multiple API instances require coordinated presence rather than the current in-process connection set; durable identity deletion/retention policy also remains follow-up work. No extra cloud service is provisioned here.


## LAN event-stream cleanup

The globe uses server-sent events (SSE), not WebSockets. LAN HTTPS can negotiate HTTP/2 with Vite while Vite forwards HTTP/1.1 to the API. A reset HTTP/2 stream can report `writableFinished`, causing the generic proxy to leave that upstream SSE request running. Repeated reloads then consume the per-visitor stream limit despite only a few visible tabs.

The session proxy explicitly destroys the upstream globe request whenever the browser response closes or aborts, and removes its cleanup listeners when the request ends. A regression test repeatedly opens and closes real HTTPS HTTP/1.1 and HTTP/2 streams and verifies upstream connections return to zero. The test uses a temporary certificate and skips if OpenSSL is unavailable.

After pulling this fix, restart the frontend and backend once to load the proxy cleanup and clear pre-existing in-memory streams. Close unused Atlas tabs. The existing limits (8 streams per visitor, 32 total) remain; hitting legitimate capacity now returns handled HTTP 429 with `Retry-After`, rather than an unhandled exception. Repeated client failures back off from 5 seconds to at most 30 seconds, resetting after a successful update.
