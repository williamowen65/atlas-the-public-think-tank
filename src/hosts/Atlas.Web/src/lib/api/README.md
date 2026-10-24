# Frontend API helpers

This folder connects frontend components to Atlas API data. It contains two main things:

- **TypeScript types and interfaces** describe the request and response data the frontend expects, such as `DiscoveryFilters`, `DiscoveryPage`, `NodeType`, and `Comment`. They describe the parts of the backend contract used here, not the entire backend API.
- **Request helpers** call endpoints, build query parameters, handle HTTP failures, and sometimes adapt responses into component data. For example, `readDiscovery` fetches a page, while `discoveryCard` converts one result into `NodeCardData` for display.

TypeScript checks how our code uses these types during development. A type annotation or `as DiscoveryPage` assertion does **not** validate incoming JSON at runtime. Some helpers perform basic response checks, but those checks are not exhaustive validation of every field. The backend remains responsible for enforcing its authorization and input-validation rules.

## Files

| File               | Purpose                                                                                                                                               |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `discovery.ts`     | Discovery filters, paginated results, and conversion into node-card display data.                                                                     |
| `nodePage.ts`      | Node details, content blocks, type catalog, ancestor and reaction reads, and public URL checks.                                                       |
| `apiRequest.ts`    | Shared JSON request transport, session cookies, antiforgery tokens for mutations, and user-facing HTTP errors.                                        |
| `communities.ts`   | Community data types and public/session endpoint helpers.                                                                                             |
| `privateDrafts.ts` | An exception: saves drafts in this browser’s `localStorage`, partitioned by participant. It does not call the backend or sync drafts between devices. |

Components generally use these helpers rather than repeat endpoint and response-handling logic. Where a helper accepts an API base, that base selects the routing prefix (for example, `/atlas-public-api` or `/atlas-session-api`); the helper appends the endpoint path. The base itself does not grant permission—the backend enforces access.
