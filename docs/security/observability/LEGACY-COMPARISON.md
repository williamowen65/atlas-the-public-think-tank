# Developer note: legacy user history and current audit capture

Both implementations inspect **Entity Framework's ChangeTracker at the save boundary**. The legacy implementation already used this mechanism; it did not subscribe to every SQL command or database read. The current design changes the integration, payload and transaction timing rather than introducing change tracking for the first time.

Source links below are pinned to `d716218f70294d06e71d171336542b1d2493df96` so the comparison remains reviewable.

## What the legacy application did

[ApplicationDbContext inherits UserHistoryDbContext](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/legacy/www.thepublicthinktank.com/Data/DbContext/ApplicationDbContext.cs#L32-L42), which in turn inherits IdentityDbContext. Its [SaveChanges overrides](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/legacy/www.thepublicthinktank.com/Data/DbContext/UserHistoryDbContext.cs#L40-L97) snapshot tracked entries before the base save: each DTO retains the entity reference, EF state and a dictionary of original property values. This is not a deep clone of the entity.

After the base save returns a positive count, the context filters Added/Modified/Deleted entries to [five trackable types](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/legacy/www.thepublicthinktank.com/Data/DbContext/UserHistoryDbContext.cs#L101-L108): AppUser, IssueVote, SolutionVote, Issue and Solution. It resolves a separate UserHistoryProcessor through a DI scope, builds history rows, adds them to the context and [calls SaveChanges again](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/legacy/www.thepublicthinktank.com/Data/DbContext/UserHistoryDbContext.cs#L110-L170) to persist them. UserHistory itself is not a trackable type, so it does not create another history entry.

The [processor](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/legacy/www.thepublicthinktank.com/Data/DbContext/UserHistoryDbContext.cs#L176-L315) builds human-readable action text containing titles or vote values, may read related content, clears user-history cache entries and obtains actor IDs from HTTP claims or entity-based fallbacks. This is application user history, with different payload needs from the new security telemetry contract.

## Compare the designs

| Concern | Legacy user history | Current observability/audit |
|---|---|---|
| Save integration | Inherited UserHistoryDbContext overrides | AtlasDataContext overrides explicitly call a separate static AuditCapture.Stage helper |
| Separate processing | UserHistoryProcessor already handles history construction | AuditCapture handles transition classification and staging; the context owns persistence/publication |
| What is inspected | Tracked entries are snapshotted, then selected changed types are processed | DetectChanges, snapshot entries, explicitly map selected changed row types |
| Timing | Save business changes first, construct history, then perform another save | Stage required audit/outbox rows before the base save, in the business transaction |
| Evidence | UserHistory rows with display-oriented action text | Bounded TelemetryEvent; selected transitions also require AuditRecord + AuditOutbox rows |
| Privacy policy | Trackable-type filter, but no equivalent explicit telemetry privacy-policy discriminator or envelope validator in this history path; action text can contain titles and vote values | Explicit Standard / ReadNoIdentityOrTarget policies, allowlisted serialization and validation; isolated node-view context |
| Actor/context | HTTP context claims or entity fallback | Ambient operation context established by the host; operation ID and verified actor when available |
| After commit | History construction follows base save; base save completion alone does not prove an outer transaction committed | Transaction owner confirms commit before publishing staged diagnostics; worker delivers audit copies later |
| Coverage | Selected tracked mutations, not all reads or SQL commands | Selected tracked mutations; other instrumentation covers selected workflows. Direct SQL business writes bypass transition capture |

## Privacy rules are another design change

The legacy user-history path selects entity types, but it has no equivalent explicit telemetry privacy-policy discriminator or output-envelope validator. Its action text deliberately includes content titles or vote values associated with a user. This observation is specific to that history implementation, not a claim that the entire legacy application had no privacy controls.

The current system makes collection rules explicit in code:

- **Standard** permits approved actor/target identifiers and bounded transition metadata for operational and audit evidence. This is not anonymous telemetry; identifiers still require appropriate access and retention controls.
- **ReadNoIdentityOrTarget** applies to NodeView. The envelope validator rejects actor, target and related-target IDs, along with mutation metadata, for these events. The node-view scope also creates a fresh operation ID and root trace, then restores the enclosing context, preventing direct linkage to the authenticated parent command.
- **Allowlisted output** projects specific fields rather than serializing entities or arbitrary properties. Titles, vote values, credentials and exception messages are not part of this event envelope.

These are enforced telemetry collection rules, distinct from a user-facing privacy notice. See [EventJson validation and projection](../../../src/Atlas.Observability/EventJson.cs#L16-L60) and [Telemetry context and node-view isolation](../../../src/Atlas.Contracts/Observability/Telemetry.cs). Missing identity/target fields on NodeView are intentional policy, not incomplete instrumentation.

## Why the timing matters

Without an enclosing transaction spanning both saves, the legacy structure can persist business changes before history construction or its second save fails. This is a consequence of the two-save ordering; it is not a claim that every legacy caller lacks an outer transaction.

The current [AtlasDataContext save path](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/src/Atlas.Persistence/AtlasDataContext.cs#L58-L128) stages required audit evidence and delivery intent before the base save. They persist atomically with business rows. Other mapped changes produce diagnostics without durable audit rows. The [publication method](https://github.com/williamowen65/atlas-the-public-think-tank/blob/d716218f70294d06e71d171336542b1d2493df96/src/Atlas.Persistence/AtlasDataContext.cs#L40-L56) relies on its caller to commit first; it cannot independently verify the commit outcome.

The separation is therefore **an inherited save hook plus post-save history processing** in legacy, versus **an explicit helper invoked from save overrides, pre-save audit staging, and after-commit diagnostic publication** now. Both designs still use inheritance to override EF's save methods, and both delegate some work to another class. The new helper is passed the context; it does not save, commit or publish on its own.

See [implementation](IMPLEMENTATION.md), [local operation and limits](LOCAL-DEVELOPMENT.md), and the [process/UML diagrams](../../blackboards/workflows/Observability/DIAGRAMS.md) for current behavior. This comparison documents the legacy implementation without changing it.
