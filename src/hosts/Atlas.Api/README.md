# Atlas.Api

ASP.NET Core JSON host for the shared Atlas application. See the [API guide](../../../docs/api/README.md), [workflow map](../../../docs/api/WORKFLOWS.md), and [security responsibilities](../../../docs/api/SECURITY.md).

No views, SPA assets, automatic migrations, or frontend dependencies.

Public Discovery accepts an optional `NodeId` query parameter for exact node-card navigation. It intersects other filters and preserves moderation visibility; `IncludeArchived=true` allows an archived ancestor to be displayed. Missing or hidden IDs return an empty Discovery page.
