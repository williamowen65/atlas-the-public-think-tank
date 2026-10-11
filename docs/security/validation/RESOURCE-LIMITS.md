# User-controlled value and resource limits — PTT-122

This is the initial review for [PTT-122](https://thepublicthinktank.atlassian.net/browse/PTT-122), based on main after PR #337 (commit `6c2abbd`). It inventories current enforcement and proposes the next implementation slice. Proposed numbers below are review choices, **not enforced limits yet**. PTT-122 remains in progress until agreed limits and boundary tests are implemented.

## How this differs from the first validation slice

PTT-121 asked whether values, references and operations are valid: is an ID empty, does a parent exist, would the graph contain a cycle, and can a failed operation leave partial writes?

PTT-122 also asks whether **the amount of otherwise valid work is reasonable**:

- A valid Markdown block can still be too long.
- A document can contain distinct, valid block IDs and still contain too many blocks.
- A graph can be acyclic and still require expensive ancestry traversal.
- Returning three results does not mean only three records were read or ranked.

Text lengths were already part of validation. This review makes their coverage explicit and adds collection, total-payload and workload concerns. Authorization, transactions and application locks continue to serve their existing purposes; they are not substitutes for resource limits.

## Where a limit belongs

| Enforcement location | Responsibility | Example |
|---|---|---|
| Domain value object or aggregate | Enforce a product rule on construction, mutation and restored state | Bio length; number of blocks in one document |
| Application operation | Enforce a budget requiring several objects or repository queries | Total text across a document's referenced blocks |
| SQL mapping and schema | Backstop stored values and relationships | A bounded text column; FK or unique index |
| Future HTTP transport | Bound incoming bytes and request activity before expensive parsing/work | Request body size, uploads, rates, cancellation |
| Query/infrastructure design | Bound work performed, not just the size of the returned list | Database pagination, traversal budget, timeout |

### What HasMaxLength does and does not do

[AtlasDataContext](../../../src/Atlas.Persistence/AtlasDataContext.cs) currently maps many labels to `nvarchar(450)` and many longer fields to `nvarchar(max)`.

- `HasMaxLength` describes the EF property mapping. With a bounded SQL column, SQL provides storage enforcement; this is not a friendly domain validation check.
- A domain rule can be stricter: Node titles allow 200 characters while their SQL column allows 450.
- A bio mapped to `nvarchar(max)` is still limited to 500 by the Participant domain. The column does not supply that product limit.
- Limiting one column cannot limit the number of block/link/history rows, aggregate text across blocks, or frequency of operations.
- Changing the SQL mapping requires a migration to change an existing database. Inspect existing data before narrowing columns; do not silently truncate it.

Lengths below use C# `string.Length` (UTF-16 code units), usually after the domain's trimming/normalization. These are not HTTP byte limits or counts of visible emoji/graphemes. Normalization policy must be preserved when adding tests.

## Existing text limits

These values are **enforced in inspected domain code**. A source link is inspection evidence, not a claim that every maximum has an exact-boundary test.

| Value | Current maximum | Enforcement source | SQL shape |
|---|---:|---|---|
| Node title | 200 | [NodeTitle](../../../src/Atlas.Graph/Nodes/NodeTitle.cs) | `nvarchar(450)` |
| Participant display name / bio | 80 / 500 | [Participant](../../../src/Atlas.Participants/Participants/Participant.cs) | Both `nvarchar(max)` |
| Community name / description | 100 / 1,000 | [Community](../../../src/Atlas.Communities/Communities/Community.cs) | Name 450; description max |
| Node type name / description | 50 / 500 | [NodeTypeDefinition](../../../src/Atlas.Graph/Nodes/NodeTypes/NodeTypeDefinition.cs) | Name 450; description max |
| Comment body | 10,000 | [Comment](../../../src/Atlas.Comments/Comments/Comment.cs) | `nvarchar(max)` |
| Markdown block text | 100,000 | [MarkdownTextBlock](../../../src/Atlas.Content/Blocks/Types/MarkdownTextBlock.cs) | `nvarchar(max)` |
| Image URL / alt text / caption | 500 / 2,000 / 2,000 | [ImageBlock](../../../src/Atlas.Content/Blocks/Types/ImageBlock.cs) | Payload fields max |
| Video URL / caption | 500 / 2,000 | [VideoBlock](../../../src/Atlas.Content/Blocks/Types/VideoBlock.cs) | Payload fields max |
| Link preview title / description | 500 / 2,000 | [LinkPreviewBlock](../../../src/Atlas.Content/Blocks/Types/LinkPreviewBlock.cs) | Payload fields max |
| Chart reference title | 500 | [ChartReferenceBlock](../../../src/Atlas.Content/Blocks/Types/ChartReferenceBlock.cs) | `nvarchar(max)` |
| Reaction definition text / description | 24 / 160 | [ReactionDefinition](../../../src/Atlas.Graph/Reactions/ReactionDefinition.cs) | Text/description max; normalized key 450 |
| Moderation report reason / explanation | 100 / 2,000 | [ModerationCase](../../../src/Atlas.Moderation/ModerationCase.cs) | `nvarchar(max)` |
| Moderation decision / restoration rationale | 2,000 / 2,000 | [ModerationCase](../../../src/Atlas.Moderation/ModerationCase.cs) | `nvarchar(max)` |

Keep these existing product limits initially. A stricter database backstop can be considered separately; it is not necessary to rewrite every mapping to begin PTT-122.

## Gaps and proposed starting bounds

“No explicit cap” means no Atlas product/resource cap was found in the inspected path. Runtime, SQL or URI implementation limits may still exist.

| Value or structure | Current position | Proposal for review | Owner and reason |
|---|---|---|---|
| Blocks per document | [Document](../../../src/Atlas.Content/Documents/Document.cs) checks nulls/duplicates/order, but has no count cap | **100 blocks** | Content aggregate: substantial composition without thousands of child rows per save |
| Total text per document | Each block has its own limit; no summed budget | **500,000 code units** across Markdown, URLs, labels, alt text and captions | Application service resolving blocks: per-block caps alone permit very large documents |
| Link preview URL | Absolute HTTP(S) syntax via [HttpUrl](../../../src/Atlas.Content/Blocks/ContentBlock.cs); no explicit length cap | **2,048 code units**, checked before URI parsing | Content: give stored preview URLs an explicit budget; review the existing 500 image/video URL limits separately |
| Reaction emoji | Required/trimmed; no explicit length cap | **32 code units** | ReactionDefinition: allow multi-code-point emoji without treating the field as free-form text; this does not validate emoji semantics |
| Parent IDs per Node | Reference/cycle checks, no count cap | **20 direct parents** | Graph aggregate: allow multiple viewpoints while bounding immediate fan-in; does not bound total ancestry |
| Requested sub-node types per Node | Validated proposals, no count cap | **50 distinct types** | Graph aggregate: keep one proposal reasonably sized |
| Discovery search text | No explicit length cap | **200 code units** | Discovery entry point: bound filter input before reading candidates |
| Discovery reaction filter IDs | No explicit count cap | **24 distinct IDs** | Discovery entry point: bound one query's filter set; reject duplicates consistently |
| Notification delivery attempts / error text | History and timestamps checked, no explicit count/text caps | **10 retained attempts / 2,000 code units per error** | Notification service/domain: decide retention and retry policy before implementation; avoid silently discarding audit history |
| Reaction audit history, comment thread size/depth, report groups, memberships, associations | No general growth cap in reviewed paths | **Separate workload/retention decisions** | Avoid hard lifetime caps that prevent moderation, leaving a community or legitimate history growth |

The proposed document budget is roughly 1 MB of UTF-16 text before object/SQL overhead; it is not an HTTP request-size recommendation. One hundred maximum-size Markdown blocks would total ten million code units, so a count cap and an aggregate budget address different risks.

### Important composition details

A Document stores block IDs, not block payloads. That affects enforcement:

- A count limit belongs in its constructor/reconstitution and `AddBlock`, before changing its timestamp or collection.
- Read at most the configured count plus one from an incoming enumerable before rejecting it. Calling `ToList()` on an arbitrarily large input first defeats the allocation budget.
- A total-text limit needs a service that resolves the blocks through Content's repository port.
- Editing an existing block can increase a document's total without changing its block IDs. The policy must cover block edits and whether blocks can be shared across documents, not just document creation.
- Deleting/reordering blocks should remain possible at the cap. Existing oversized saved documents need an explicit remediation strategy if restored-state checks become stricter.
- A per-document cap does not cap how many documents, blocks or Nodes one account can create over time. Account quotas and request-rate controls are separate decisions.

## Query and workload gaps

[DiscoveryService](../../../src/Atlas.Discovery/DiscoveryService.cs) returns pages of three, but ranks the full candidate set before slicing. Its [Console candidate source](../../../src/Atlas.Console/Discovery/RepositoryDiscoveryCandidateSource.cs) reads Nodes and related data. A small visible page is not a database work limit.

[NotificationService.Page](../../../src/Atlas.Notifications/Notifications.cs) accepts limits from 1 to 100, and [SqlNotificationRepository.Page](../../../src/Atlas.Console/Storage/SqlNotificationRepository.cs) applies SQL ordering/Skip/Take. This bounds returned rows; large offsets and included delivery histories can still make work expensive.

[SqlNodeRepository](../../../src/Atlas.Console/Storage/SqlNodeRepository.cs) loads all Nodes for some queries, and graph ancestry checks can visit many ancestors. Limiting direct parents does not bound graph depth or total visited Nodes. The graph lock also means a long topology operation can delay other topology operations.

Before choosing arbitrary graph-size or lifetime limits, plan separately:

- SQL-backed Discovery filtering/ranking/pagination that preserves current ranking behavior.
- Traversal limits, cancellation and useful errors when an operation exceeds its budget; never accept unchecked relationships after an early traversal stop.
- Paged comment threads and moderation groups, plus retention for delivery/audit history.
- Query timeouts and performance evidence using realistic data sizes.

Do not truncate candidate sets silently: doing so can make ranking, totals and reference/cycle checks incorrect.

## First implementation slice to review

1. Retain existing text maxima; replace repeated literal bounds with named constants where useful.
2. Implement document block-count limits, then parent/requested-type collection limits at all relevant entry points.
3. Add explicit link-preview URL, reaction emoji and Discovery filter bounds.
4. Add focused boundary tests and link their exact assertions here.
5. Design aggregate document-text enforcement before promising that budget. Record larger query/retention changes as linked follow-up tasks.
6. Consider SQL column alignment after the product bounds settle; any schema changes need migrations and existing-data checks.

These numbers are proposals, not universal best practices. Review them against intended Atlas compositions and realistic examples before enforcing them.

## Evidence and test plan

Existing examples to reuse:

- [ParticipantTests](../../../tests/Atlas.Participants.Tests/ParticipantTests.cs): `Constructor_WithBioOverMaximumLength_Throws` and `UpdateProfile_WithInvalidBio_DoesNotPartiallyRename`.
- [ContentValidationTests](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs): `InvalidPayloadDoesNotPartiallyUpdateBlocks` and composition rejection tests. These do not establish a document count/total-size budget.
- [DiscoveryServiceTests](../../../tests/Atlas.Discovery.Tests/DiscoveryServiceTests.cs): page/range behavior. These do not establish bounded candidate-reading cost.

For every implemented new bound, test:

- Exactly the maximum succeeds; maximum plus one fails.
- Constructor, mutation and reconstitution apply the agreed policy.
- Rejection preserves fields, collections, timestamps and domain events.
- Application checks reject oversized queries before invoking the candidate source.
- SQL workflows roll back any prior writes on a resource-limit failure where that workflow can save before encountering it.
- Count/total-payload limits are tested independently; one does not prove the other.

This document records inspected code and proposed work. It does not claim new enforcement or new passing tests.

## Deferred HTTP and media controls

Coordinate with PTT-125 and future API work:

- Request body bytes, JSON depth, multipart/upload bytes, request rates and timeouts.
- Authentication-aware quotas where a per-object limit cannot control cumulative account activity.
- Cancellation before/during expensive queries and graph traversal.
- URL-fetch response bytes, redirect/time budgets and destination policy if previews are fetched remotely.
- Image/video storage limits: current blocks store references, so a short URL says nothing about remote media size.

Keep the current backend bounds authoritative. The future API can reject oversized requests earlier, but UI or transport controls must not be the only enforcement for product limits.
