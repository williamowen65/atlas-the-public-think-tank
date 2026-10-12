# Node creation quota — PTT-133

Draft implementation for [PTT-133](https://thepublicthinktank.atlassian.net/browse/PTT-133). **The provisional default is 3 Nodes per rolling 2 hours**, chosen within the requested 1–3-hour range. Review the allowance and window before merging; this is not a settled product policy.

## Policy and user feedback

- Root Nodes and sub-Nodes count equally; comments and metadata edits do not count.
- Each successful creation occupies one slot until exactly its creation-accounting time plus the window. There is no single daily/hourly reset.
- Failed transactions consume nothing. Archiving, hiding or removing a Node does not refund its creation.
- Console displays remaining allowance before creation, reports the next slot time in UTC, and refreshes remaining allowance after success.
- Exhaustion includes the limit/window, an approximate rounded-up minute wait and the exact next-slot UTC timestamp.
- `NodeCreationQuotaExceededException.Allowance` exposes structured data (`Limit`, `Window`, `Remaining`, `CheckedAt`, `NextSlotAt`, `RetryAfter`) for future UI/API handling. UI can convert timestamps to the user's timezone and maintain a countdown.
- The allowance display is advisory. Another request may consume a slot before the user submits; the save always rechecks atomically.

## Where the code lives

| Piece | Responsibility |
|---|---|
| [NodeCreationQuotaPolicy](../../../src/Atlas.Graph/Nodes/NodeCreationQuota.cs) | Configurable immutable policy and pure rolling-window calculation |
| [INodeCreationQuota](../../../src/Atlas.Graph/Nodes/NodeCreationQuota.cs) | Optional repository capability for allowance/validation; shared host-facing data contract |
| [NodeCreationService](../../../src/Atlas.Console/NodeCreationService.cs) | Early check inside the topology transaction, before Content writes |
| [SqlNodeRepository](../../../src/Atlas.Console/Storage/SqlNodeRepository.cs) | Rechecks every new repository insert and records consumption; updates do not consume |
| [NodeCreationQuotaRow](../../../src/Atlas.Persistence/NodeCreationQuotaRow.cs) | Durable creation ledger, independent of Node lifecycle |
| [NodeCreationWorkflow](../../../src/Atlas.Console/NodeCreationWorkflow.cs) | Console display and friendly quota errors |

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

`AddNodeCreationQuota` creates an indexed ledger and backfills existing authored Nodes using their stored creation timestamps. Recent existing Nodes therefore count at rollout; older ones fall outside the window. Past physically deleted Nodes cannot be reconstructed. Decide whether that rollout policy is wanted before merging.

Ledger rows intentionally have no cascading Node FK: deleting/archiving a Node cannot erase its consumption. The rows currently have no automated cleanup. A retention plan may purge sufficiently old rows after deciding the maximum supported window and account-deletion/privacy policy. This is history storage, not an unbounded query result: allowance reads are limited in SQL.

There is no automatic ordinary-repository exemption. SQL inserts through `SqlNodeRepository` count; seeding/import code writing directly through DbContext is privileged infrastructure and can bypass this application policy. Review those paths explicitly. The optional interface does not impose SQL quotas on non-SQL/in-memory repository implementations.

## Tests and limits

- [NodeCreationQuotaTests](../../../tests/Atlas.Graph.Tests/NodeCreationQuotaTests.cs): last slot, exact expiry, tightened policy and structured/user-readable rejection.
- [SqlNodeCreationQuotaTests](../../../tests/Atlas.Persistence.Tests/SqlNodeCreationQuotaTests.cs): exhaustion, archive without refund, participant isolation, exact expiry, rollback, competing last-slot requests and direct-save/backdated input protection.
- Existing SQL creation/rollback/concurrency tests remain part of CI. The new migration is checked against the EF model by SQL CI.

Clock rollback may extend the wait because future ledger timestamps remain counted; quotas do not synchronize server clocks. Cache, HTTP rate throttling, multi-account abuse controls and reputation/karma-based reply permissions are separate features. The quota slows sustained creation but permits a burst up to the allowance; add a minimum spacing rule only if that is a separate agreed product requirement.

## Questions for review

1. Is **3 Nodes per rolling 2 hours** the right starting policy, or should it be 3 per hour / 3 per 3 hours / another combination?
2. Should root Nodes and sub-Nodes share one allowance as implemented?
3. Is allowing all available slots in a short burst acceptable, or do you also want a minimum delay between creations?
4. Should recent existing Nodes count when the migration is applied?
5. Which seed/import/administrator operations need explicitly trusted exceptions?
6. What ledger retention/account-deletion policy should follow this draft?

No karma/reputation threshold is included. A Node-specific setting requiring reputation to reply/create sub-Nodes needs a separate authorization/product proposal.
