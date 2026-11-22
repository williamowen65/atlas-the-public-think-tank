# Public-page SEO

[PTT-168](https://thepublicthinktank.atlassian.net/browse/PTT-168)

## Existing deployment and scope

`AtlasDeploy.yml` builds Atlas.Web with SvelteKit's static adapter, writes directly to
the ASP.NET Core host's `wwwroot`, and deploys one same-origin app.
The root SvelteKit layout disables SSR. The API host serves the SPA shell for
known UI routes and now adds route metadata to the initial HTML.

| URL | Proposed first release behavior |
| --- | --- |
| `/` | Index the Discover landing page. Omit sort and filter parameters from its canonical URL. |
| `/about` | Index with a dedicated title and description. |
| `/nodes?id=<id>` | Index eligible nodes. Keep the ID in the canonical URL and drop contribution, comment, and filter parameters. |
| `/nodes` without a valid ID | Exclude from indexing. Verify the existing empty-state behavior before choosing its HTTP status. |
| `/communities/<id>`, `/profiles?id=<id>` | Share active public communities and profiles with top-section screenshots. Keep them out of indexing and the sitemap. |
| Community directory, personal account, draft, moderation, editor, and component routes | Exclude from the sitemap and return noindex on HTML responses. Existing access rules govern what the app can show. |

## Delivery decision

| Approach | What it delivers | Deployment impact |
| --- | --- | --- |
| Prerender selected SvelteKit routes | Initial HTML for stable public pages. Dynamic nodes would need regeneration on publication and moderation. | Static deployment can remain, but a build-time node snapshot would need invalidation. |
| SvelteKit SSR for public routes | Initial metadata and rendered page content, including text search engines can read directly. | Introduces a server runtime and deployment routing. Audit browser-only loads and session cookie handling. |
| Generate public HTML in the ASP.NET Core host | Initial metadata for existing URLs using live public data. A metadata-only first slice supports link previews while the SPA still renders the body. | Keeps the existing artifact and same-origin deployment. Requires a public HTML response path and a separate way to verify body indexing. |

The first release uses the ASP.NET Core host for metadata on the home feed,
About, and public node links. Share previews use the Atlas title and the page’s public description. When
capture is enabled, each preview points to a screenshot of the shared page.
Discover captures include the page intro and wait for the first card’s content
and controls. Node captures frame the shared card. Both omit site navigation
and voting error controls. The sharing service controls which fields it displays.
SvelteKit still renders the page body in the browser. Server-rendered article
content remains a separate decision for search discovery.

## Eligibility and metadata

Use the anonymous public read rules even when the request has an authenticated
session. The node API can show a hidden original to an authorized actor and
returns a placeholder to other readers. The HTML metadata and sitemap should
share an eligibility rule based on publication, active status, and moderation
visibility. Generate titles and descriptions from eligible public revisions only.
Use plain text from published content blocks, HTML-encode values, and use an Atlas
fallback description and branded social image when public content is missing or
unsuitable. Begin with one branded image and add eligible node images later if
needed. Emit the document title, description, canonical URL, Open Graph tags,
and Twitter card tags in the initial response.

A valid but missing node ID returns 404. The shell still lets the browser
render the missing-node screen. A hidden node can show its existing moderation
notice and an archived node can remain reachable by direct link. Both receive
generic Atlas metadata and noindex while the archived indexing policy remains
open. The sitemap also excludes them. There is no shared cache for this HTML.
API authorization and moderation rules still govern access to the actual
content, independent of crawler directives.

Signed-out direct visits to drafts, settings, personal profile, and moderation
redirect to the feed. Signed-in visitors can still open their own pages. These
pages use noindex and generic Atlas metadata, so a preview never contains a
draft title, settings, or account data. Public entry points such as login and
registration still work via direct link. Unknown routes and missing files
return 404.

## Host and rollout

The live SEO origin determines the canonical production hostname. An optional
live redirect hostname permanently redirects to that origin, preserving the
path and query. Without either setting, the site uses its Azure hostname and
has no redirect alias. Terraform only configures application settings here.
The `site_urls` output reports the Azure-provided URLs.

1. Set up Azure custom hostname bindings, DNS, and HTTPS for the two live
   hostnames and the demo hostname. Bind `www` to the live app so it can issue
   the redirect.
2. In the GitHub environment used for Terraform plan and apply, set
   `ATLAS_SEO_PUBLIC_ORIGINS` to
   `{"live":"https://atlas.thepublicthinktank.com","demo":"https://demo.thepublicthinktank.com"}`
   once those bindings are ready. Set `ATLAS_SEO_LIVE_REDIRECT_HOST` to
   `www.thepublicthinktank.com` if using that bound alias. Terraform defaults each unset origin to its
   App Service hostname. Keep `ATLAS_SEO_INDEXING_ENABLED` false for the
   first rollout.
3. Deploy the branch to demo and verify the two share cards below. Demo and
   local send `X-Robots-Tag: noindex` on HTML responses and have no sitemap.
   Their robots file permits crawling so clients can read the noindex header.
4. After checking the production origin and alias over HTTPS, enable
   `ATLAS_SEO_INDEXING_ENABLED` through Terraform. The application only allows
   indexing and exposes the sitemap when the request hostname matches the
   configured canonical live site. The sitemap reads current public node IDs,
   capped at 49,998 nodes plus the home and About URLs. Add sitemap partitions
   before reaching that limit.
5. Verify the public site in Search Console and submit the canonical sitemap
   after production validation.

## Verify direct links

Run `npm run build` in `src/hosts/Atlas.Web`, then start the ASP.NET host and
open `https://localhost:7240/`. The frontend build writes directly to the API
web root. To capture the built app with local Chromium, set
`Atlas__Seo__CaptureOrigin` to `https://localhost:7240` before starting the API. Vite serves its own development shell and is useful for
checking the interactive app. Use an HTTP client to inspect the initial HTML
and headers without running JavaScript. For example, on Azure demo:

```sh
curl -si https://demo.thepublicthinktank.com/
curl -si 'https://demo.thepublicthinktank.com/nodes?id=PUBLIC_NODE_ID&type=comments'
curl -si https://demo.thepublicthinktank.com/my-drafts
curl -si https://demo.thepublicthinktank.com/robots.txt
curl -si https://demo.thepublicthinktank.com/sitemap.xml
```

Check that the node canonical keeps the ID and drops contribution parameters.
Paste the home and node URLs in the intended sharing service. Check a
moderated node, an archived node, a missing node, and a private account route.
Repeat the moderated request with an authorized session to ensure its head
stays generic. Verify that the share image loads and that browser navigation
clears metadata from the previous direct response. Repeat checks for both
HTTPS production hosts and compare sitemap contents before submission.

## Browserless credential for website screenshots

Add `BROWSERLESS_IO_API_KEY` under the GitHub Actions environment secrets for
`atlas-infra-apply`. Terraform passes the same credential to the live and demo
website apps. An absent secret leaves the setting unset. The key is carried
in protected Terraform state and plan data, like the SMTP password.

Atlas connects to `wss://production-sfo.browserless.io` over CDP in Azure
and uses local Chromium for development. The API keeps the token server-side.
Each website process queues up to eight distinct images and runs one capture
at a time. Browserless enforces the account limit across live and demo.
Terraform creates a dedicated Azure Storage account and private share-image
container for each website. Each website uses its existing runtime managed
identity to read and write its own container. Atlas serves the images through
the existing share-image URLs. Azure removes cache blobs after seven days
without modification. The previous App Service filesystem cache is not migrated;
Azure generates fresh images when requested.

## Animated globe capture on Azure

The animated share card is 1200 × 630. The v15 experiment plays 1,440 frames at an average
of 12 fps over a two-minute rotation. Playback duration is independent of
capture time.

- **Browserless capture.** A single 720-frame session hit the two-minute
  free-tier session limit. Two 360-frame sessions also ran close to that
  limit on demo. The confirmed 6 fps version used four groups of 180 frames.
  The 12 fps experiment uses eight groups of 180 frames in
  fresh, sequential sessions, reusing the same pin snapshot throughout.
  The successful four-session capture in demo logged about 42–63 seconds
  per session. Each session pays its own browser startup and page-load cost.
- **Azure compositing and encoding.** After capture, Atlas places each
  globe frame on the share-card background, then compresses the composited
  frames into GIF data. Keeping all 720 composited frames resident coincided
  with peaks of 97% memory and 100% CPU on the shared demo plan. The v14
  encoder processes 24 frames at a time, appends the compressed frames to
  one GIF, and releases each batch before continuing. Demo served the
  resulting animation successfully. Batching reduces retained frame memory.
  Azure still has to composite and compress every captured frame.
- **Verify a deployment.** Open the animated globe share page
  (`/?globe=open&motion=gif`) and request its advertised `og:image` URL.
  On a cache miss, Atlas waits up to 20 seconds and may serve a PNG
  fallback while the capture continues. Once ready, the URL returns
  `Content-Type: image/gif`. For v17, the published blob is
  `v17/globe-motion.gif` in the site's private `share-images` container.
- **Diagnose a slow or failed run.** In App Service Log stream, look for
  eight `captured 180 frames` messages, then `captured 1440 frames`.
  The v17 encoder reduces the batch size to six frames. Atlas reports
  progress after the first batch and every 30 frames with
  elapsed time and process working set. The `saved as v17/globe-motion.gif`
  message follows upload. `BlobNotFound` is expected on cache reads
  before publication. Use app-level **Memory Working Set** and plan-level
  **Memory Percentage** and **CPU Percentage** around the same timestamps
  to distinguish process growth from pressure across the shared worker.
  Local tests verify GIF structure and timing. Use Azure demo for
  Browserless session timing, worker resource usage, and blob publication.

## Capture screenshots locally

Start the Atlas API and the web development server with published sample data.
From `src/hosts/Atlas.Web`, install local Chromium once and capture Discover
or a published node by its UUID:

```sh
npx playwright install chromium
npm run capture:share
npm run capture:share -- --node 00000000-0000-0000-0000-000000000001
```

Replace the example UUID with a node from your local database. Discover defaults
to a wide viewport and nodes use the same compact card as the feed. The
node capture view is available at `/nodes?id=PUBLIC_NODE_ID&share=feed`. Use
`--layout mobile` or `--layout wide` to compare 1200 × 630 outputs, and
`--origin http://localhost:5173` to select a different loopback origin.
The capture runs in a signed-out local Chromium session and writes to
`src/hosts/Atlas.Web/.share-captures/`. Git ignores this directory and the web
build does not publish it. Set `ATLAS_LOCAL_CHROMIUM` only to override the
Playwright-managed browser with a local executable.

The manual commands generate files for development review. To exercise the
automatic HTTP flow, use the ASP.NET host that serves the SPA and its Open Graph
metadata. Install Chromium for the API's Microsoft.Playwright version by running
`pwsh bin/Debug/net10.0/playwright.ps1 install chromium` after building
`src/hosts/Atlas.Api`. Set `Atlas__Seo__CaptureOrigin` on the development API
to the loopback origin Chromium should open (default `https://localhost:7240`).
The server cache defaults to `src/hosts/Atlas.Web/.share-captures/server/`
when the API is run from its project directory. The API capture also accepts
`ATLAS_LOCAL_CHROMIUM` as a local browser executable override.

When a reader requests a share image, Atlas reuses a current PNG from its cache.
A missing or stale image queues one capture per target. The request waits up to
seven seconds, then serves the static Atlas fallback
while the queued capture continues. Node edits invalidate captures via the
node and document timestamps. Home captures expire after 24 hours and are also
invalidated by moderation decisions. Clients may cache their own link previews.


To verify community and profile cards, open an active community and a public
profile signed out, check the Open Graph title and description, and open each
`og:image` URL. The capture shows the public top section. The image endpoints
return 404 for archived communities or inactive profiles. These pages remain
out of the sitemap and send `noindex`.

On a published node card or detail page, click the link icon next to the
byline. Paste the copied URL and confirm it opens the node without extra
query parameters. If clipboard access is unavailable, the button exposes
the URL in a selectable field.
