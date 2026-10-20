# Audit delivery: leases, claims and acknowledgements

AuditDelivery moves a **copy of an already committed audit event** from SQL to a destination. SQL remains the authoritative history. It does not perform the original business action or create its audit evidence.

Think of AuditOutbox as a shipping dock: AuditRecordRow is the package's stored contents, while AuditOutboxRow is its destination-specific delivery ticket. The current local console runs one delivery loop per LocalDevelopmentTelemetry instance. The leasing protocol also handles competing processes: if multiple workers look at tickets, each must claim a temporary lease before attempting delivery.

## How many workers run today?

**The current local console starts one background delivery task per LocalDevelopmentTelemetry instance.** Normal console startup creates one instance, so one loop processes audit tickets sequentially. This is not a pool of parallel delivery workers, nor does each ticket start a new task.

[LocalDevelopmentTelemetry](../../../src/Atlas.Application/Observability/LocalDevelopmentTelemetry.cs) starts that loop with `_delivery = Task.Run(async () => { ... })`. A task is a logical unit of asynchronous work, not a dedicated thread: continuations may use different thread-pool threads, and awaiting database I/O or Task.Delay does not occupy a thread waiting for completion.

The loop behaves as follows:

| Outcome | What the single loop does next |
|---|---|
| DeliverOneAsync returns true | Immediately checks again, whether the previous attempt delivered, scheduled a retry, or lost a claim race |
| DeliverOneAsync returns false | Waits 500 ms because no ticket is currently eligible; the queue may still contain delayed or leased entries |
| An unhandled failure reaches the host | Emits a bounded delivery-failure diagnostic, then waits 1 second before retrying |
| Shutdown is requested | Cancellation ends the loop; Dispose synchronously waits for task completion before removing the observer and disposing local telemetry |

The Console loop checks backlog health approximately every 10 seconds; there is no separate health-check worker.

One sequential loop provides the current local host's simple delivery lifecycle without managing parallel workers. The code does not claim a measured throughput requirement or performance advantage for this choice.

The Development API host also starts **one loop per API process**, implemented by
[ApiAuditWorker](../../../src/hosts/Atlas.Api/ApiTelemetry.cs) as a hosted background
service. It uses the same AuditDelivery protocol and shared SQL outbox. Running
Console and API together therefore means two independent loops may compete for
eligible tickets; the database lease still decides which attempt owns each ticket.
API shutdown uses hosted-service cancellation. Unlike the Console adapter, this
initial API worker does not yet sample backlog health periodically. PTT-136 owns
production export and operational health wiring.

### Why use leases with just one loop?

One loop per console instance is not the same as one loop for the whole database. Two console processes can connect to the same database, and the API host or a test can create another worker. The conditional SQL claim coordinates those independent attempts. Lease expiry also recovers abandoned work after a process stops or crashes.

The A/B worker examples below explain that contention and recovery protocol; they do **not** mean the current host launches two workers. Leases reduce overlapping claims while valid, but expiry during slow delivery can still allow overlap and duplicate output.

Shutdown waits for the worker to stop; it does **not** drain the entire outbox or guarantee every pending item reaches a file before exit. Committed pending entries remain in SQL for a later run, and an abandoned lease can expire so processing resumes.

## The three terms

| Term | Meaning in this file | What it does not mean |
|---|---|---|
| Lease | A temporary reservation represented by LeaseOwner and LeaseUntilUtc | A database lock held throughout file/network I/O, or an absolute guarantee against duplicate delivery |
| Claim | An atomic conditional SQL update that acquires the lease and increments Attempts | Merely reading a candidate row |
| Acknowledgement | A conditional SQL update setting DeliveredAtUtc after the destination accepts the payload | Deleting the event, acknowledging every destination, or guaranteeing the file exists forever |

## What the file contains

- **IAuditDestination** defines a destination name and DeliverAsync. Successful completion means durable acceptance; failure must throw.
- **LocalAuditDestination** writes the stored PayloadJson through JsonLineFile, which flushes the local write to disk. It returns Task.CompletedTask because that work is synchronous.
- **AuditDelivery** receives a context factory and a destination. DeliverOneAsync handles at most one candidate per call. LocalDevelopmentTelemetry owns the repeated background loop and shutdown.

## 1. Find an eligible ticket

[Candidate selection](https://github.com/williamowen65/atlas-the-public-think-tank/blob/fde8a51c93c07e6c2cae00f9118be0e581975a65/src/Atlas.Persistence/Observability/AuditDelivery.cs#L58-L62) filters for this destination, no delivery timestamp, a retry time that has arrived, and an absent or expired lease. Earliest NextAttemptAtUtc comes first; Id breaks ties.

Reading the row does not reserve it. AsNoTracking requests a read-only EF snapshot; it does not prevent another worker from reading it.

If none is currently eligible, the method returns false. That does not prove the outbox is empty: tickets may be delayed or leased.

## 2. Claim a lease

[Conditional lease update](https://github.com/williamowen65/atlas-the-public-think-tank/blob/fde8a51c93c07e6c2cae00f9118be0e581975a65/src/Atlas.Persistence/Observability/AuditDelivery.cs#L74-L79) repeats the eligibility checks inside one SQL UPDATE. It sets a fresh owner GUID, increments Attempts and assigns a two-minute expiry. ExecuteUpdateAsync returns the number of affected rows; it executes immediately without SaveChanges.

| Step | Worker A | Worker B |
|---|---|---|
| Read | Sees pending ticket R | Also sees pending ticket R |
| Claim | Updates R; claimed = 1 | Rechecks R after A's update; claimed = 0 |
| Continue | May attempt delivery | Skips delivery and returns true so the host can try again |

The owner GUID identifies this claim attempt, not the original user or a permanent worker identity.

A lease is a cooperative reservation encoded in SQL. Expiry makes the ticket eligible for a later claim; no timer actively clears it. If A crashes, another worker can reclaim it after expiry. The lease is not renewed here: if A remains alive but delivery takes too long, B can reclaim and delivery can overlap.

## 3. Load and deliver the stored event

After a successful claim, the worker reloads the ticket so Attempts includes this attempt, then loads its linked AuditRecordRow.

[Destination delivery](https://github.com/williamowen65/atlas-the-public-think-tank/blob/fde8a51c93c07e6c2cae00f9118be0e581975a65/src/Atlas.Persistence/Observability/AuditDelivery.cs#L93-L93) sends the original stored payload. It does not rebuild the event from current business rows or replace the original actor with the worker. Delivery diagnostics have their own System operation context and reference the audit record ID.

File/network I/O happens outside the original business transaction. The business change is already committed.

## 4. Acknowledge this destination

[Acknowledgement update](https://github.com/williamowen65/atlas-the-public-think-tank/blob/fde8a51c93c07e6c2cae00f9118be0e581975a65/src/Atlas.Persistence/Observability/AuditDelivery.cs#L98-L100) matches both the ticket ID and owner token, sets DeliveredAtUtc and clears the lease.

- acknowledged = 1: the matching ticket was updated; emit a succeeded delivery diagnostic.
- acknowledged = 0: ownership no longer matches; emit a conflict diagnostic.

The token prevents an old claimant from acknowledging a ticket now owned by another claimant. The acknowledgement checks ownership, not the expiry timestamp itself.

After acknowledgement, subsequent candidate queries exclude that ticket because DeliveredAtUtc is non-null. A worker already in flight may still finish a duplicate delivery. A separate cloud ticket remains pending until its own destination accepts it; a local receipt does not count as cloud acceptance. No cloud adapter is implemented in this branch.

## 5. Recover from failures

**Example: crash before delivery.** A claims R and exits before writing. Its lease expires; B claims R and tries again.

**Example: crash after delivery but before acknowledgement.** A flushes event E to disk and crashes before setting DeliveredAtUtc. B later replays E. Both lines carry the same stable event_id. Consumers can use that ID to deduplicate; this implementation does not automatically deduplicate local files.

**Example: destination throws.** The handler schedules NextAttemptAtUtc, clears only its own lease and emits a bounded failure diagnostic. It stores retry state rather than exception text. Delays are 2, 4, 8, 16, 32, 64, 128 and then 256 seconds for subsequent attempts. Although the expression also has a 300-second ceiling, the exponent cap makes 256 seconds the effective maximum. There is no maximum retry count that deletes evidence.

**Example: cancellation.** Requested cancellation propagates and leaves the lease to expire. Cancellation before the file write is checked; an already-started synchronous file write is not interrupted.

Database failures during selection/claim/loading, or during retry-state persistence, can propagate to the host. A successfully claimed ticket remains recoverable through lease expiry. The host decides when to invoke the method again.

## Understand the return value

| Result | Interpretation |
|---|---|
| false | No candidate is currently eligible |
| true | A candidate was found: delivery may have succeeded, failed with retry scheduled, or lost the claim race |
| Exception | Cancellation or an unhandled failure escaped to the host |

**true does not mean delivered.** The SQL acknowledgement and delivery diagnostic distinguish completion from contention or failure.

## Guarantees and limits

This is a transactional-outbox delivery path designed for **at-least-once delivery**, assuming workers and the destination eventually become available. Duplicates are possible; exactly-once delivery is not promised. Lease expiry does not stop an existing worker. Audit files may later rotate away, while SQL remains authoritative; acknowledgement records acceptance at delivery time rather than perpetual file availability.

See the [process/UML diagram guide](../../blackboards/workflows/Observability/DIAGRAMS.md), [local-development guide](LOCAL-DEVELOPMENT.md), and [legacy comparison](LEGACY-COMPARISON.md). Source links above are pinned to commit fde8a51c93c07e6c2cae00f9118be0e581975a65.

## Account lockout evidence

When Identity persists a transition from no active lockout to a future `LockoutEnd`,
`AuditCapture` records `identity.account_security_changed` with action `lockout_started`.
The affected account GUID is the target; an unsuccessful sign-in does not establish a
verified actor. Failed-attempt counter changes, clearing a lockout and extending an
already active lockout use `lockout_changed`. Natural expiry does not itself write a record.
These records and their outbox tickets commit with the Identity update. Email addresses,
passwords and password hashes are excluded. This adds an action value, not a database column.

`SqlAuditTests.Fifth_failed_attempt_starts_lockout_with_private_data_excluded_from_audit`
exercises Identity's failed-attempt mechanism: four failures do not start a lockout;
the fifth commits one `lockout_started` record and delivery ticket. It also checks privacy
and that extending the active lockout does not report another start. This is an automated
integration test, not another background task. Rejected sign-ins against an already
locked account remain separate `SignIn` / `Denied` / `LockedOut` diagnostic events.
