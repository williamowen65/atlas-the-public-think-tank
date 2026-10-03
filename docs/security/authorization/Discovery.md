# Discovery Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Discover public nodes](../../../src/Atlas.Discovery/DiscoveryService.cs) | Public | No actor required | `DiscoveryService` filters/ranks public candidates and excludes moderation-hidden candidates | Discovery tests + anonymous Console | Covered |
| [Filter/search public nodes](../../../src/Atlas.Discovery/DiscoveryService.cs) | Public | No actor required | Discovery validates query ranges and applies text/community/type/author/status/reaction/vote/date filters | Discovery tests | Covered |
| [Page public results](../../../src/Atlas.Discovery/DiscoveryService.cs) | Public | No actor required | Discovery page validation + fixed page size | Discovery service/tests | Covered |
| [View authored-node listing](../../../src/Atlas.Discovery/DiscoveryService.cs) | Public profile/contribution view under current policy | No actor required | Host supplies explicit authored node IDs; Discovery scopes to those IDs and retains moderated placeholders | `DiscoverAuthored*` | Covered under current policy |
| [Discover hidden original content](../../../src/Atlas.Discovery/DiscoveryService.cs) | Not through ordinary Discovery | Actor is relevant only through separate moderation/author workflow | Ordinary discovery excludes `IsModerationExcluded`; hidden-original access is controlled by Moderation | Discovery + Moderation | Covered separation |
| [Use filters to bypass moderation exclusion](../../../src/Atlas.Discovery/DiscoveryService.cs) | Not permitted | No actor can elevate ordinary Discovery | Candidate exclusion occurs before search/ranking for ordinary discovery | `DiscoveryService.DiscoverCore` | Covered |

## Review note

Discovery is primarily a public read boundary, so most operations intentionally have no actor. Its security responsibility is chiefly **visibility filtering**: ordinary search must not reveal content excluded by Moderation.
