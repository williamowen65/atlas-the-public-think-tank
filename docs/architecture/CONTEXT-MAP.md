# Atlas current-state context map

Atlas is a modular application hosted by Atlas.Console. Domain projects own behavior and repository contracts; Console composes them and supplies SQL repository adapters. Atlas.Persistence owns one shared EF Core context, persistence rows, Fluent API mappings, and SQL Server migrations.

## Components and ownership

| Component | Owns / current role | References and limits |
|---|---|---|
| Graph | Typed Nodes, parent links, requested response types, curated reaction definitions and node-reaction lifecycle | Author, description, and type IDs; author checks are domain-enforced. Graph relationship services and SQL adapters check transitive ancestry; topology changes share an application lock. |
| Content | Documents, ordered block references, typed payload validation, identity and timestamps | Poll/chart references have no mapped principal table. Block addition and node creation coordinate related saves in one transaction. |
| Participants | Public profiles, active state, protected self-edit use case | Identity owns credentials and sign-in; Participants owns profile state and authorized self-edit. |
| Voting | Node importance ratings (0–10), reaction votes (+1/−1), change/undo, current summaries | Identifier-based eligibility/availability ports; SQL uniqueness protects participant-target pairs. |
| Communities | Community metadata/lifecycle, membership, node associations | Participant and Node IDs; owner authorization and separate records for memberships/associations. |
| Comments | Threaded comments, single parent comment, lifecycle and permissions | Author and polymorphic target IDs; Comments are separate from Graph children. |
| Discovery | Search/filter/rank policy and ranked result IDs | Host materializes candidates from repositories; no mutable Graph aggregate ownership. |
| Moderation | Reports, decisions, public reasons, review requests, restoration | Stored global Identity roles supply authorization; grouped changes share a transaction. |
| Notifications | Recipient feed, preferences, read/dismiss state, delivery attempts | Occurrence/recipient uniqueness; email/push are simulated and events are synchronous. |
| Contracts | Versioned Graph lifecycle and notification-request payloads | Versioned payloads plus shared clock, reference and operation ports; no SQL storage ownership. |
| Console | UI, authenticated actor, workflows, adapters, startup and event registration | Identity-backed sign-in supplies the actor; a future HTTP host will compose its own request scope. |
| Persistence | AtlasDataContext, row types, mappings, constraints, migrations | Shared SQL Server infrastructure; domain projects do not depend on EF Core. |

## Persistence flow

Repository interfaces remain in their owning domains. The implementation chain is:

1. Console calls the domain behavior or application service.
2. A Sql*Repository maps the resulting domain object into an EF row.
3. SqlRepository starts or joins the operation context/transaction, prepares owned child keys, and saves.
4. SQL Server enforces configured primary keys, foreign keys, and unique indexes.
5. On read, the adapter loads rows/owned children and reconstitutes domain objects without replaying creation events.

Related repository calls inside an operation boundary share one transaction. Standalone calls do not automatically join earlier saves. Full saves replace owned children; Node metadata saves preserve parent rows. Serializable isolation, constraints and topology coordination protect competing writes, while universal stale-object version checks remain deferred. See [transaction responsibilities](DATA-OWNERSHIP.md#transaction-boundaries).

## Cross-boundary references

Graph stores AuthorId and DescriptionId rather than Participant/Document objects. SQL foreign keys now enforce those references when present. Owned children cascade on physical owner deletion; independent references use NoAction and block deletion while dependents remain. Archive/restore changes status rather than physically deleting rows.

Vote TargetId, Comment TargetId, and Notification SubjectId are polymorphic; their kind determines the target table, so ordinary foreign keys do not enforce those targets. Poll/chart references, occurrence IDs, and opaque type owner strings also remain outside ordinary FK enforcement.

## Startup and events

Program.cs reads User Secrets, then environment variables. ATLAS_SQL_CONNECTION_STRING configures SqlStorage, which applies checked-in migrations. Optional --seed-demo inserts typed demo rows into an empty database and exits. Normal startup constructs repositories/services and Identity-backed session services, ensures system Node types, registers subscribers, and starts ConsoleApplication with anonymous browsing and sign-in/registration.

| Event | Registration / current effect |
|---|---|
| NodeCreatedV1 | Console Content observer checks that the description exists. |
| NodeArchivedV1 | Console Content observer handles the lifecycle fact. |
| NodeRestoredV1, NodeParentAttachedV1, NodeParentDetachedV1 | Graph records them; no dedicated Content observer registration is added for these types. |
| NotificationRequestedV1 | NotificationService handles recipient requests through SqlNotificationRepository. |

The host publishes recorded events after relevant saves. Dispatch is synchronous and in memory. Subscriber failure can interrupt dispatch after data is committed; no durable outbox/inbox or retry queue exists. Notification row deduplication is implemented, but it does not make event transport or external delivery durable.

## Creation consistency

[NodeCreationWorkflow](../../src/Atlas.Console/NodeCreationWorkflow.cs) collects input and delegates to [NodeCreationService](../../src/Atlas.Console/NodeCreationService.cs). The service constructs the proposed block, document and Node before writing, validates references and ancestry, and saves Content then Graph inside one topology-coordinated SQL transaction. A later save failure rolls back earlier writes. The host publishes recorded events after the service returns successfully; dispatch is outside the transaction. See [workflow evidence and limits](../security/validation/IMPLEMENTATION.md#workflows-and-evidence).

## Current gaps

- Keep new topology entry points behind graph coordination; database foreign keys alone do not prevent cycles.
- Define relationship-node cardinality and global type-name concurrency rules.
- Define a general stale-object/version policy and host retry behavior for translated operation conflicts.
- Extend rollback/failure evidence when new multi-save workflows are added; existing coordinated workflows already share a transaction.
- Add durable integration messaging, future HTTP/browser authentication boundaries, rate limits, and real delivery adapters.
- Optimize materialized Discovery/ancestry queries and adopt stable feed pagination where needed.
- Complete event compatibility/metadata and lifecycle policy before service extraction.

See [Data ownership](DATA-OWNERSHIP.md), [SQL setup](../PTT-87-SQL-Server.md), [requirements](../requirements/REQUIREMENTS.md), and [traceability](../requirements/TRACEABILITY.md).
