# Local visitor globe (PTT-162 / PTT-163)

The desktop corner globe opens a nearly full-screen visitor map. On phones, Globe lives in the existing bottom navigation. Escape or the close button dismisses the dialog and returns focus to its launcher. Live / Historical controls switch counts and pins together.

This implementation is a **local development preview**. The widget is mounted only by Vite development builds; API endpoints return 404 outside `Development`. No deployment, Terraform change, cloud service, startup migration, or keep-alive request is included.

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

## Generate the local migration

`src/Atlas.Persistence/AGENTS.md` requires developer-generated migrations and forbids the agent from generating/applying them. The EF model changes are included; the migration, designer and model snapshot deliberately remain unchanged.

From the solution root, with `ATLAS_SQL_CONNECTION_STRING` set to your **local SQL Server** (or shared user secrets pointing there):

```sh
dotnet tool restore
dotnet ef migrations add AddGlobeVisitorHistory --project src/Atlas.Persistence
```

Review the generated migration. It should add `GlobeVisitors` and `GlobeBrowsers`, including the browser-to-visitor foreign key, without changing existing data tables. Commit the migration, designer and snapshot together. Then:

```sh
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter FullyQualifiedName~SqlGlobeHistoryTests
dotnet test tests/Atlas.Api.Tests/Atlas.Api.Tests.csproj --filter FullyQualifiedName~Globe
```

Do not target an Azure connection string or run the Azure setup/deployment workflows for this work. Migration-dependent persistence tests and the full local SQL experience remain unverified until this developer step is complete. Existing SQL test setup also needs the generated migration before its migration-based checks can pass.

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
npx playwright test --config playwright.globe.config.ts
npm run build
```

The normal Playwright configuration excludes the development-only globe spec, since it serves the production build. API tests separately exercise real SSE responses with independent cookie jars and an in-memory history adapter; these do not establish SQL durability. SQL persistence tests exercise merging, restart/repository reload, pin aggregation and removal after the migration is generated.

## Implementation and later deployment work

The reference is William Owen's [GeographicPinMap](https://github.com/williamowen65/GeographicPinMap) at `68febc15e1b9f0509445339744f9e7c07def16c2`. Its D3 orthographic projection, rotating globe and explicit pin interaction are adapted into Svelte components. The original Express/Socket.IO server is replaced with a .NET server-sent event listener; the feature requires only server-to-browser updates, while normal CSRF-protected HTTP writes handle visits and location changes.

The map asset is vendored Natural Earth public-domain land geometry; no runtime CDN, Glitch endpoint or external map request is used. D3 geometry is bundled through npm. See `src/hosts/Atlas.Web/static/globe/README.md` for attribution.

Live presence is **single API process** and intentionally capped at 100 streams total / 8 per visitor. Each subscriber has a bounded, latest-snapshot queue; disconnects and cancellations clean up listeners. Reconnection is automatic and unavailable live data is not presented as current. Multiple API instances, durable identity deletion/retention policy, production throttling/capacity and production feature enablement require a later deployment design before exposing this publicly. No extra cloud service is provisioned here.
