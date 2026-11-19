# Validation

This first review slice, [PTT-121](https://thepublicthinktank.atlassian.net/browse/PTT-121), implements the validation workstream of [PTT-97](https://thepublicthinktank.atlassian.net/browse/PTT-97). It reviews current MVP backend operations and Console coordination; it is not a claim that every invalid state is now prevented.

Validation answers whether a proposed value or operation is valid. Authorization answers whether the caller may perform it. Both are required, and a future form's dropdowns or validation messages cannot replace backend enforcement.

- [Implementation overview](IMPLEMENTATION.md): the implemented workflows, test evidence and practical limits.
- [Domain review](DOMAIN-REVIEW.md): expectations, enforcement locations, automated evidence and remaining gaps across the MVP domains.
- [Persistence and SQL](PERSISTENCE.md): EF parameterization, database constraints and their limits.
- [Resource limits review](RESOURCE-LIMITS.md): existing bounds, unbounded collections/workloads and proposed PTT-122 limits.
- [Authorization review](../authorization/README.md): caller identity and permissions.

## Review method

Review constructors, reconstitution, mutation methods, services, Console adapters and SQL repositories. For each entry point:

- **Check values:** null/default values, empty IDs, undefined enums, text normalization and bounds, and finite numbers.
- **Check collections and state:** duplicates, ordering, lifecycle rules and timestamps.
- **Check relationships:** missing references and whether the referenced record is available for the operation.
- **Check failure behavior:** rejection should not leave partial changes; coordinated SQL writes should roll back together.
- **Check the evidence:** distinguish an inspected code path from a rule actually asserted by a test. Call backend methods directly so Console choices cannot conceal a gap.

Track each rule as **expectation → owning enforcement → test or inspected code → fix or follow-up**.

Reuse existing tests rather than duplicating them. A linked test file identifies an evidence source; it does not prove every possible input combination.

For column meanings, reference validity and trust between layers, see [Reading the inventory](DOMAIN-REVIEW.md#reading-the-inventory).

## Layer responsibilities

| Layer | Responsibility |
|---|---|
| Console / future HTTP transport | Parse GUID strings and numeric input, report useful errors, establish a trusted actor. A typed `Guid` can still be empty. |
| Value object / aggregate | Validate local values and invariants, including direct calls and reconstitution; reject before changing fields, collections or events. |
| Application service / host coordination | Resolve references and availability through owning ports; check repository-wide relationships; coordinate atomic writes. |
| SQL / EF | Parameterize values; enforce mapped FK and unique constraints as a final integrity check. A FK proves existence, not authorization or active status. |
| Future browser / URL fetcher | Safely render Markdown and other stored text; enforce a URL-fetch policy if remote previews are introduced. These are separate from SQL safety. |

Domain entry points still need checks because:

- **Default IDs:** `default(NodeId)` creates a struct without running its validating constructor, so its GUID can be empty.
- **Null objects:** Nullable annotations produce compiler warnings; they do not prevent a caller from passing null at runtime.
- **Saved data:** Reconstitution rebuilds an object from stored values. Those values must satisfy domain rules too; SQL constraints do not enforce every rule.

These local-value and state-consistency changes are implemented in this PR and tracked by [PTT-128](https://thepublicthinktank.atlassian.net/browse/PTT-128).

## Initial review fixes

| Finding | Change | Regression evidence |
|---|---|---|
| Block updates assigned fields before validating later fields/time | All six block types validate proposed payload into locals, validate time, then assign | [ContentValidationTests.InvalidPayloadDoesNotPartiallyUpdateBlocks](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs) and [StaleTimeDoesNotChangeAnyBlockPayload](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs) |
| Block time could move backward after a previous edit | Compare against current [UpdatedAt](../../../src/Atlas.Content/Blocks/ContentBlock.cs); equal timestamps remain accepted | [ContentValidationTests.StaleTimeDoesNotChangeAnyBlockPayload](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs) |
| Composition mutated before a stale timestamp failed | Add/move/remove validate time before changing the list | [ContentValidationTests.StaleTimeDoesNotChangeDocumentComposition](../../../tests/Atlas.Console.Tests/ContentValidationTests.cs) |
| `NaN` escaped rating-range comparisons | Reject all non-finite Discovery rating bounds before reading candidates | [DiscoveryServiceTests.DiscoverRejectsNonFiniteBoundsBeforeReadingCandidates](../../../tests/Atlas.Discovery.Tests/DiscoveryServiceTests.cs) |
| Undefined lifecycle/disposition enum integers were accepted | Reject undefined Node, Community, Comment and reaction/audit enums at relevant domain entry points | [Graph ValidationTests](../../../tests/Atlas.Graph.Tests/ValidationTests.cs), [Communities ValidationTests](../../../tests/Atlas.Communities.Tests/ValidationTests.cs) and [Comments ValidationTests](../../../tests/Atlas.Comments.Tests/ValidationTests.cs) |
| Decided moderation records could have no decision timestamp or an oversized rationale | Reconstitution now requires time and the same 2,000-character normalized rationale bound as decisions | [ValidationTests.DecidedCaseRequiresDecisionTimeAndBoundedRationale](../../../tests/Atlas.Moderation.Tests/ValidationTests.cs) |
| Communities tests were absent from the rewrite CI matrix | Add the existing test project to CI | [.github/workflows/AtlasRewriteTests.yml](../../../.github/workflows/AtlasRewriteTests.yml) |

## Current workflow summary

The follow-up implementation adds these protections to the initial fixes:

- **Complete operations share a transaction.** Related writes commit together or roll back together. See the [workflow and evidence table](IMPLEMENTATION.md#workflows-and-evidence).
- **Creation and parent changes share the graph lock.** This protects cycle checks across nodes. Metadata edits use [SaveMetadata](../../../src/Atlas.Graph/Nodes/INodeRepository.cs), preserve stored parents and do not request that lock. Ordinary SQL locks still apply.
- **Tests cover specific failure and concurrency paths.** These include a real SQL document-insert rejection after the block was saved, error propagation to the caller, and rollback. Other tests cover competing graph changes, metadata writes while the graph lock is held, and stale metadata saves preserving newer parents.

For the boundaries of these guarantees, read [Limits to retain during review](IMPLEMENTATION.md#limits-to-retain-during-review). For how domain checks, application workflows and SQL constraints work together, read [data integrity and its layers](PERSISTENCE.md#what-data-integrity-means).

No EF model or migration changes are required by this slice.

## Follow-ups included in this PR

The three follow-up cards are now part of PR #337 on the PTT-121 branch:

| Tracking | What to review | Evidence and policies |
|---|---|---|
| [PTT-128](https://thepublicthinktank.atlassian.net/browse/PTT-128) | Default/null guards, chronological changes, restored-state consistency and a shared UTC clock | [Local values and history](IMPLEMENTATION.md#local-values-and-history--ptt-128) |
| [PTT-129](https://thepublicthinktank.atlassian.net/browse/PTT-129) | Backend reference availability, graph ancestry, target kinds and notification shortcuts | [References and relationships](IMPLEMENTATION.md#references-and-relationships--ptt-129) |
| [PTT-130](https://thepublicthinktank.atlassian.net/browse/PTT-130) | Shared SQL transactions, rollback, competing writes and useful conflict errors | [Complete operations](IMPLEMENTATION.md#complete-operations--ptt-130) |

Suggested reading order:

1. [Implementation overview](IMPLEMENTATION.md): workflows, shared clock, transactions and linked test scenarios.
2. [Domain review](DOMAIN-REVIEW.md): individual expectations, enforcement and gaps.
3. [Persistence](PERSISTENCE.md): SQL safety, integrity layers and their limits, plus [browser and network concerns](PERSISTENCE.md#browser-and-network-concerns).

- [Node creation quota draft](NODE-CREATION-QUOTA.md): provisional PTT-133 policy, concurrency, migration and review questions.

## Remaining work

| Work | Tracking |
|---|---|
| Collection sizes, text/resource budgets and query limits | [PTT-122](https://thepublicthinktank.atlassian.net/browse/PTT-122) |
| Broader failure-path coverage | [PTT-123](https://thepublicthinktank.atlassian.net/browse/PTT-123) |
| Browser rendering, request validation and any future outbound URL fetcher | [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125) |

Keep this inventory current when follow-ups are implemented. Do not promote an inspected rule to automated evidence without a corresponding assertion.
