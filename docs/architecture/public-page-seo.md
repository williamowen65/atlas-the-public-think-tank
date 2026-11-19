# Public-page SEO proposal

[PTT-168](https://thepublicthinktank.atlassian.net/browse/PTT-168)

## Where the response is served today

`AtlasDeploy.yml` builds Atlas.Web with SvelteKit's static adapter, copies its
output into the ASP.NET Core host's `wwwroot`, and deploys one same-origin app.
The root SvelteKit layout disables SSR. The API host serves the same SPA shell
for known UI routes, including `/nodes` even when the node does not exist.
Discover already sets a title and description in Svelte, but those route-level
tags are populated by the browser.

| URL | Proposed first release behavior |
| --- | --- |
| `/` | Index the Discover landing page. Omit sort and filter parameters from its canonical URL. |
| `/about` | Index with a dedicated title and description. |
| `/nodes?id=<id>` | Index eligible nodes. Keep the ID in the canonical URL and drop contribution, comment, and filter parameters. |
| `/nodes` without a valid ID | Exclude from indexing. Verify the existing empty-state behavior before choosing its HTTP status. |
| `/communities`, `/communities/<id>` | Defer until anonymous access, eligibility, and stable community URLs have been verified. |
| Profile, account, draft, moderation, editor, and component routes | Exclude from the sitemap and return noindex on HTML responses. Existing access rules govern what the app can show. |

## Delivery options

| Approach | What it delivers | Deployment impact |
| --- | --- | --- |
| Prerender selected SvelteKit routes | Initial HTML for stable public pages. Dynamic nodes would need regeneration on publication and moderation. | Static deployment can remain, but a build-time node snapshot would need invalidation. |
| SvelteKit SSR for public routes | Initial metadata and rendered page content, including text search engines can read directly. | Introduces a server runtime and deployment routing. Audit browser-only loads and session cookie handling. |
| Generate public HTML in the ASP.NET Core host | Initial metadata for existing URLs using live public data. A metadata-only first slice supports link previews while the SPA still renders the body. | Keeps the existing artifact and same-origin deployment. Requires a public HTML response path and a separate way to verify body indexing. |

Start with a demo spike in the ASP.NET Core host. Serve the same initial HTML to
people and crawlers. Add a single set of server-visible metadata, and verify that
Svelte navigation replaces it without leaving stale or duplicate tags. Measure
the initial HTML before committing to a discovery approach. If meaningful body
content is required in the first release, compare a server-rendered public
article with selective SvelteKit SSR before building out the sitemap.

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

Proposed status policy is a 404 for a missing node and a generic noindex shell
for a moderated node if the public moderation notice still needs to render.
Keep archived nodes accessible by direct link when current product rules allow
it, and exclude them from the sitemap with noindex applied to their pages.
Verify that public metadata disappears as soon as visibility changes. During
the spike, avoid shared caching for dynamic HTML and distinguish a transient
backend failure from a missing node.

## Host and rollout decisions

- Use `atlas.thepublicthinktank.com` as the canonical host if `www` may later
  serve a separate project site. If both hostnames serve Atlas, configure the
  alias to permanently redirect to the canonical host while preserving paths
  and queries. If Atlas is the primary website at `www`, that host is an
  equally valid canonical choice.
- Configure the public origin and the indexing switch independently for each
  deployment. Default indexing to off. On demo and local, generate canonical
  and social URLs from the configured environment origin, send
  `X-Robots-Tag: noindex` on HTML, and omit the sitemap. Let crawlers retrieve
  demo pages so they can see noindex. Use access controls if demo content needs
  to be private.
- Serve the production sitemap only on the canonical production host. Generate
  it from the public eligibility rule and advertise its absolute URL in
  `robots.txt`. Gate indexing on an explicitly configured production origin.
- Provision DNS, Azure custom hostname bindings, and HTTPS certificates for the
  selected hosts, then verify the HTTPS alias behavior. After production
  validation, verify a Search Console domain property via DNS if registrar
  access is available and submit the canonical production sitemap there.

## Verification for the first implementation

Use an HTTP client without JavaScript against the local packaged ASP.NET host
and the deployed demo hostname. Check the raw head, status, and headers for
`/`, `/about`, an active public node, the same node with contribution parameters,
a hidden node, an archived node, a missing node, and a private or account route.
Repeat the hidden case with an authorized session and verify that the HTML head
contains no hidden original. Check the robots response and sitemap behavior
under both indexing configurations, including exclusion of demo URLs and
ineligible node IDs. Verify the demo share card and its image in the intended
sharing service, then check browser navigation for duplicate or stale head tags.
Repeat checks for both HTTPS production hosts before sitemap submission.
