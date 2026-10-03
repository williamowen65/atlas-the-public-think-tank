# Validation

This first review slice, [PTT-121](https://thepublicthinktank.atlassian.net/browse/PTT-121), implements the validation workstream of [PTT-97](https://thepublicthinktank.atlassian.net/browse/PTT-97). It reviews current MVP backend operations and Console coordination; it is not a claim that every invalid state is now prevented.

Validation answers whether a proposed value or operation is valid. Authorization answers whether the caller may perform it. Both are required, and a future form's dropdowns or validation messages cannot replace backend enforcement.

- [Domain review](DOMAIN-REVIEW.md): expectations, enforcement locations, automated evidence and remaining gaps across the MVP domains.
- [Persistence and SQL](PERSISTENCE.md): EF parameterization, database constraints and their limits.
- [Authorization review](../authorization/README.md): caller identity and permissions.

## Review method

Review public constructors, reconstitution paths, mutation methods, services, Console adapters and SQL repositories. For each entry point consider null/default values, empty IDs, undefined enum casts, text normalization and bounds, finite numbers, collection duplicates/order, missing references, lifecycle rules, timestamps and partial changes on rejection. Inspect tests as evidence; distinguish code inspection from an automated assertion. Use backend methods directly in regression tests so Console choices cannot conceal a gap.

The traceability unit is **expectation → owning enforcement → test or inspected code → fix or follow-up**. Existing tests are reused rather than duplicated. A test filename below identifies an evidence source, not proof of every possible input combination.

## Layer responsibilities

| Layer | Responsibility |
|---|---|
| Console / future HTTP transport | Parse GUID strings and numeric input, report useful errors, establish a trusted actor. A typed `Guid` can still be empty. |
| Value object / aggregate | Validate local values and invariants, including direct calls and reconstitution; reject before changing fields, collections or events. |
| Application service / host coordination | Resolve references and availability through owning ports; check repository-wide relationships; coordinate atomic writes. |
| SQL / EF | Parameterize values; enforce mapped FK and unique constraints as a final integrity check. A FK proves existence, not authorization or active status. |
| Future browser / URL fetcher | Safely render Markdown and other stored text; enforce a URL-fetch policy if remote previews are introduced. These are separate from SQL safety. |

A successful constructor check is insufficient for a record-struct ID: `default(T)` bypasses its constructor. Nullable reference objects can also be passed at runtime despite nullable annotations. Reconstitution is a validation entry point, not an exemption for database data.

## Changes in this slice

| Finding | Change | Regression evidence |
|---|---|---|
| Block updates assigned fields before validating later fields/time | All six block types validate proposed payload into locals, validate time, then assign | `ContentValidationTests.InvalidPayloadDoesNotPartiallyUpdateBlocks` and `StaleTimeDoesNotChangeAnyBlockPayload` |
| Block time could move backward after a previous edit | Compare against current `UpdatedAt`; equal timestamps remain accepted | `ContentValidationTests.StaleTimeDoesNotChangeAnyBlockPayload` |
| Composition mutated before a stale timestamp failed | Add/move/remove validate time before changing the list | `ContentValidationTests.StaleTimeDoesNotChangeDocumentComposition` |
| `NaN` escaped rating-range comparisons | Reject all non-finite Discovery rating bounds before reading candidates | `DiscoveryServiceTests.DiscoverRejectsNonFiniteBoundsBeforeReadingCandidates` |
| Undefined lifecycle/disposition enum integers were accepted | Reject undefined Node, Community, Comment and reaction/audit enums at relevant domain entry points | Graph, Communities and Comments `ValidationTests` |
| Decided moderation records could have no decision timestamp or an oversized rationale | Reconstitution now requires time and the same 2,000-character normalized rationale bound as decisions | Moderation `ValidationTests.DecidedCaseRequiresDecisionTimeAndBoundedRationale` |
| Communities tests were absent from the rewrite CI matrix | Add the existing test project to CI | `.github/workflows/AtlasRewriteTests.yml` |

No EF model or migration changes are required by this slice.

## Remaining work

| Work | Tracking |
|---|---|
| Required default/null values, timestamp policies, and full restored-state consistency | [PTT-128](https://thepublicthinktank.atlassian.net/browse/PTT-128) |
| Backend reference availability and transitive graph-cycle checks | [PTT-129](https://thepublicthinktank.atlassian.net/browse/PTT-129) |
| Atomic coordination of multi-repository writes, failure handling and concurrency | [PTT-130](https://thepublicthinktank.atlassian.net/browse/PTT-130) |
| Collection sizes, text/resource budgets and query limits | [PTT-122](https://thepublicthinktank.atlassian.net/browse/PTT-122) |
| Broader failure-path coverage | [PTT-123](https://thepublicthinktank.atlassian.net/browse/PTT-123) |
| Browser rendering, request validation and any future outbound URL fetcher | [PTT-125](https://thepublicthinktank.atlassian.net/browse/PTT-125) |

Keep this inventory current when follow-ups are implemented. Do not promote an inspected rule to automated evidence without a corresponding assertion.
