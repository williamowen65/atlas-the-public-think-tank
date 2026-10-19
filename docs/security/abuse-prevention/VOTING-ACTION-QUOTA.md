# Voting action allowance

[Abuse responsibilities](README.md) · [Negative-path evidence](../negative-paths/README.md) · [Voting architecture](../../architecture/VOTING.md)

[PTT-140](https://thepublicthinktank.atlassian.net/browse/PTT-140), implemented in PR #352 with PTT-123/PTT-124.

Voting owns one rolling action allowance per participant across **node ratings and reaction votes**. The provisional configurable MVP default is **60 successful actions per rolling 10 minutes**, with bursts allowed. This is a starting policy for review, not a measured production threshold.

## What counts

| Operation | Consumption |
|---|---|
| Cast a new vote | One slot |
| Change an existing vote, including resubmitting the same value | One slot |
| Successfully undo an existing vote | One additional slot; the earlier cast/change remains counted |
| Undo when no current vote exists | No slot; returns false |
| Invalid value, ineligible actor, unavailable target or failed transaction | No committed slot |
| Read a summary or allowance | No slot |

All successful actions age out independently. There is no calendar reset. Changing targets or switching between rating and reaction voting does not select a separate pool. Different participants have independent pools. Node-creation allowance is separate and unchanged.

## Why undo does not refund

This policy measures **successful actions over time**, not the number of current votes. A cast, change or undo each performs work. Refunding on undo would let someone repeatedly cast and undo while recycling the same slot, enabling vote churn without spending the budget.

For example, cast → change → undo uses three slots even though no current vote remains. That is intentionally different from a transaction failing: failed work must not leave a committed consumption entry.

The tradeoff is that when allowance is exhausted, undo also waits for a slot. The typed exception exposes the next slot time and retry duration so hosts can explain this. There is no permanent restriction: slots return as entries expire. A separate bounded undo allowance could be considered later if observed user behavior makes correction too difficult; it is not implemented here.

This limits committed database mutation work. It does **not** stop someone from sending thousands of API requests: rejected attempts and no-op undo can still reach the backend. The future broad API/edge limiter must budget attempts, reads and failures before expensive work, alongside operation-specific and global concurrency controls. See PTT-137. It must also address retry/idempotency behavior rather than assuming repeated HTTP submissions are free.

## Ownership and transaction flow

1. `CastVote` and `UndoVote` enter the optional `IVoteActionQuota` coordination capability through `VoteActionBoundary`.
2. The SQL adapter acquires a transaction-owned named lock **per participant**, before reading eligibility/current vote/allowance in that callback. Concurrent requests from the same participant therefore take turns across processes.
3. Voting checks eligibility and target availability, then checks allowance before mutating an existing vote. Missing-vote undo returns false first.
4. `SqlVoteRepository.Save` / `Delete` independently recheck under that lock, write the current vote change and append a `VoteActionQuotaRow` using server accounting time.
5. The outer transaction commits both, or rolls both back. Nested reaction-association work shares that transaction, so quota rejection does not leave a new association behind.

Direct repository writes cannot evade the SQL quota or backdate consumption by supplying an old vote timestamp. Read-only allowance is advisory; it reserves nothing. The ledger has no cascading Vote FK, so deletion cannot erase consumption. It stores only a generated action ID, participant ID and accounting timestamp. Retention/account-deletion policy belongs with PTT-139; no automatic cleanup or privileged bypass is added here.

`VoteActionQuotaPolicy` and the capability live in **Atlas.Voting**. SQL implementation lives in **Atlas.Application.Storage**, row/model mapping in **Atlas.Persistence**. Voting has no dependency on Console/HTTP/EF. Non-SQL adapters without the capability do not provide durable allowance enforcement; they must supply equivalent coordination/accounting before being used where this policy is required. Tests' simple in-memory adapters are not production quota stores.

Sources: [policy and capability](../../../src/Atlas.Voting/VoteActionQuota.cs), [CastVote](../../../src/Atlas.Voting/CastVote.cs), [UndoVote](../../../src/Atlas.Voting/UndoVote.cs), [SQL repository](../../../src/Atlas.Application/Storage/SqlVoteRepository.cs), [row](../../../src/Atlas.Persistence/VoteActionQuotaRow.cs).

## Migration: developer action required

This extension changes the EF model. Per `src/Atlas.Persistence/AGENTS.md`, no migration, designer or model snapshot is generated/edited by the assistant. **The developer must generate, review and commit these artifacts before migration-dependent tests/CI can pass.** No database update has been applied on the developer's behalf.

This PR also adds a [community creation ledger](README.md#community-creation-budget).
If neither quota migration exists yet, generate one combined migration as shown below.
If the voting migration already exists locally, keep it and generate the remaining
community change with `dotnet ef migrations add AddCommunityCreationQuota --project src/Atlas.Persistence`.
Review the existing migration history first; do not generate duplicate table creation.

From the solution root in PowerShell:

```powershell
dotnet tool restore
dotnet ef migrations add AddVotingAndCommunityQuotas --project src/Atlas.Persistence
dotnet ef migrations has-pending-model-changes --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
dotnet test tests/Atlas.Voting.Tests/Atlas.Voting.Tests.csproj
dotnet test tests/Atlas.Persistence.Tests/Atlas.Persistence.Tests.csproj --filter "FullyQualifiedName~SqlVoteActionQuotaTests|FullyQualifiedName~SqlVoteRepositoryTests"
```

For a combined migration, also verify `CommunityCreationQuotaRows` and its participant/time index. Inspect that Up creates `VoteActionQuotaRows` (Id PK, ParticipantId, OccurredAt, participant/time index), Down removes it, and the generated snapshot matches. Start with an empty ledger: existing current votes do not represent historical action counts and are not backfilled. Review migration history first if a local draft exists; do not blindly scaffold duplicate table creation. Connection strings remain in the existing user-secrets/environment configuration, never source control.

## Evidence and limitations

- [VoteActionQuotaTests](../../../tests/Atlas.Voting.Tests/VoteActionQuotaTests.cs): pure rolling policy/expiry, tightening, shared action pool, no refund, no-op and invalid input, denied change/undo preserves current state.
- [SqlVoteActionQuotaTests](../../../tests/Atlas.Persistence.Tests/SqlVoteActionQuotaTests.cs): real SQL per-participant accounting, persisted/reopened allowance, exact expiry, shared target kinds, invalid input/outer rollback, direct-save/backdating and competing final-slot requests.
- The dedicated **Voting quota fresh model** CI job uses `EnsureCreated` only for isolated test databases. It proves current-model behavior, **not an upgrade path**. Existing migration consistency checks remain enabled for ordinary SQL jobs; those should fail pending model changes until the developer-generated migration is committed.
- Server clock rollback can extend waits. Metrics use the existing bounded Denied/QuotaExceeded reason; no new event fields or content logging are introduced. Console receives the friendly rejection message but does not yet show a pre-action allowance counter. The future API should use the structured allowance for HTTP 429/retry mapping and verified identity rather than accepting actor IDs from requests.
