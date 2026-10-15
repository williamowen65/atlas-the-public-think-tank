# Node creation quota — PTT-133

Draft implementation for [PTT-133](https://thepublicthinktank.atlassian.net/browse/PTT-133). **The agreed starting policy is 3 Nodes per rolling 2 hours, with bursts allowed.**

## Policy and user feedback

- Root Nodes and sub-Nodes count equally; comments and metadata edits do not count.
- Each successful creation occupies one slot until exactly its creation-accounting time plus the window. There is no single daily/hourly reset.
- Failed transactions consume nothing. Archiving, hiding or removing a Node does not refund its creation.
- Console displays remaining allowance before creation, reports the next slot time in UTC, and refreshes remaining allowance after success.
- Exhaustion includes the limit/window, an approximate rounded-up minute wait and the exact next-slot UTC timestamp.
- `NodeCreationQuotaExceededException.Allowance` exposes structured data (`Limit`, `Window`, `Remaining`, `CheckedAt`, `NextSlotAt`, `RetryAfter`) for future UI/API handling. UI can convert timestamps to the user's timezone and maintain a countdown.
- The allowance display is advisory. Another request may consume a slot before the user submits; the save always rechecks atomically.

## Terminology

**Ledger:** A ledger is the record of successful Node creations that consumed quota. Each NodeCreationQuotaRow records the Node ID, participant ID and accounting timestamp; NodeCreationQuotaRows is the SQL table holding those records. Trusted exempt operations do not add entries, and failed transactions leave none committed. “Ledger” names this ordinary quota-history table; it is not another service or special database mechanism. See the [shared glossary](../../glossary/GLOSSARY.md).

## Where the code lives

| Piece | Responsibility |
|---|---|
| [NodeCreationQuotaPolicy](../../../src/Atlas.Graph/Nodes/NodeCreationQuota.cs) | Configurable immutable policy and pure rolling-window calculation |
| [INodeCreationQuota](../../../src/Atlas.Graph/Nodes/NodeCreationQuota.cs) | Optional repository capability for allowance/validation; shared host-facing data contract |
| [NodeCreationService](../../../src/Atlas.Application/NodeCreationService.cs) | Early check inside the topology transaction, before Content writes |
| [SqlNodeRepository](../../../src/Atlas.Infrastructure/Storage/SqlNodeRepository.cs) | Rechecks every new repository insert and records consumption; updates do not consume |
| [NodeCreationQuotaRow](../../../src/Atlas.Persistence/NodeCreationQuotaRow.cs) | One record of a successful creation that consumed quota; stored independently of Node lifecycle |
| [NodeCreationWorkflow](../../../src/Atlas.Console/NodeCreationWorkflow.cs) | Console display and friendly quota errors |

Root/sub-Node sharing is per participant: creating a child beneath someone else's Node consumes only the creator's allowance, never the parent's author's allowance.

Policy can be injected into `SqlNodeRepository`; absent injection uses the provisional default. This draft does not add a settings UI or dynamic per-account tier system. Ordinary Console calls derive the author from the authenticated actor. A future API must likewise use trusted identity rather than accepting a request's arbitrary author ID.

## Concurrency, explained

A read-only allowance check alone is insufficient: two requests could both see one remaining slot.

1. Creation enters the existing SQL topology transaction and acquires `AtlasGraphTopology` before checking.
2. It queries the participant's newest ledger entries inside that transaction.
3. If allowance remains, it saves the block, document and Node, then the ledger entry.
4. Only the outermost operation commits. Any failure rolls back Content, Node and consumption together.
5. A competing creation waits for the same graph lock. After commit it sees the new ledger entry and rejects when no slot remains.

This reuses the existing cross-instance SQL application lock; no new application-wide lock is added. It is conservative: topology operations already take turns even for different participants. If topology locking is narrowed later, quota check-and-consume must retain equivalent per-participant coordination and tests.

The ledger timestamp comes from the shared server clock, not the Node object's supplied creation timestamp. Direct repository creation cannot evade quota by backdating its Node. SQL queries return at most `Limit` history rows using an index on participant/time. If a policy is tightened, the next allowed slot depends on the Limit-th newest creation, not merely the oldest retained history entry.

## Migration and lifecycle

Migration generation belongs to the developer's local workflow. This PR changes the EF model but includes no quota migration/designer or changed model snapshot. Generate and review them locally before applying the database update.

The new table is defined by NodeCreationQuotaRow, the NodeCreationQuotaRows DbSet and model.Entity<NodeCreationQuotaRow> in AtlasDataContext. EF should scaffold its primary key, columns and participant/time index.

```bash
dotnet tool restore
dotnet ef migrations add AddNodeCreationQuota --project src/Atlas.Persistence
dotnet ef database update --project src/Atlas.Persistence
```

Inspect and commit the generated Up/Down and snapshot changes. Create an empty table, with no existing-Node backfill. SQL CI will report pending model changes until these artifacts are committed; the PR remains draft.

If an earlier draft migration was already applied locally, resolve that migration-history state first. Do not blindly apply another table-creation migration to the same database. Pulling these changes does not undo a database update.

Ledger rows intentionally have no cascading Node FK: deleting/archiving a Node cannot erase its consumption. The rows currently have no automated cleanup. A retention plan may purge sufficiently old rows after deciding the maximum supported window and account-deletion/privacy policy. This is history storage, not an unbounded query result: allowance reads are limited in SQL.

Trusted seed/import/admin operations can use `SqlNodeRepository.ExecuteTrustedCreation` around their callback, using the same repository. Validation and rollback still apply, but no user slots are checked or consumed. The host must authorize admin callers before using it; never expose this as a client flag. It does not automatically exempt every ordinary post by an administrator.

The existing `SqlStorage.SeedDemoDataAsync` path writes rows directly and already bypasses user creation checks. That privileged path remains available. Repository-based import/admin tools now have an explicit exemption capability. Non-SQL/in-memory repositories without the quota interface do not provide SQL accounting.

## Tests and limits

- [NodeCreationQuotaTests](../../../tests/Atlas.Graph.Tests/NodeCreationQuotaTests.cs): last slot, exact expiry, tightened policy and structured/user-readable rejection.
- [SqlNodeCreationQuotaTests](../../../tests/Atlas.Persistence.Tests/SqlNodeCreationQuotaTests.cs): exhaustion, archive without refund, participant isolation, exact expiry, rollback, competing last-slot requests and direct-save/backdated input protection.
- Existing SQL creation/rollback/concurrency tests remain part of CI. The new migration is checked against the EF model by SQL CI.

Clock rollback may extend the wait because future ledger timestamps remain counted; quotas do not synchronize server clocks. Cache, HTTP rate throttling, multi-account abuse controls and reputation/karma-based reply permissions are separate features. The quota slows sustained creation but permits a burst up to the allowance; add a minimum spacing rule only if that is a separate agreed product requirement.

## Ledger retention and account deletion

The ledger stores Node ID, participant ID and accounting timestamp, not post text.

- **Retention:** decide when expired entries can be purged. They no longer affect the two-hour allowance, but may be useful for audit or later policy changes. No automatic cleanup is implemented yet.
- **Account deletion:** decide whether participant-linked entries should be removed or anonymized when an account is deleted, and whether justified audit retention is needed.
- There is no cascading deletion of this history. A future deletion/cleanup workflow must apply the chosen policy explicitly so deleting a Node cannot refund a recent slot.

## Decisions and remaining review

Confirmed: 3 per rolling 2 hours; bursts allowed; roots/sub-Nodes count toward the creator's allowance; no existing-Node backfill; trusted seed/import/admin operations can bypass.

Still open: ledger retention/account-deletion handling and integration of authorized admin/import callers as those tools are introduced.

No karma/reputation threshold is included. Node-specific reputation requirements need a separate product/authorization proposal.
