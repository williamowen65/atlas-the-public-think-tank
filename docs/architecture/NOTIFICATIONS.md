# Notifications — PTT-102

Notifications owns recipient-specific notification records, preferences, read/dismissed state, delivery attempts, and the paged feed. It references Participants by ID and source content by kind and ID. Comments owns comments; Moderation owns decisions; Graph owns Nodes. Source domains do not choose message wording, channels, or recipient preferences. The console host translates completed source actions into `NotificationRequestedV1` events. A notification is one occurrence for one recipient; its `(OccurrenceId, RecipientParticipantId)` pair is the idempotency key.

## Initial workflows

| Source action | Recipient | Kind | Default |
| --- | --- | --- | --- |
| New top-level Node comment | Node author | `NodeCommented` | In-app |
| Reply to comment | Parent comment author | `CommentReplied` | In-app |
| Moderator hides Node | Node author | `NodeHidden` | In-app |
| Moderator restores Node | Node author | `NodeRestored` | In-app |
| Moderator decides report | Each report submitter | `ReportDecided` | In-app |

Self-notifications are suppressed. One reply notifies only its direct parent author; a top-level comment notifies only the Node author. A recipient can toggle in-app, simulated email, and simulated push by Discussion or Moderation category. No email address or device token is stored here. The simulated adapters make zero network calls and record an attempt only when selected. A failed adapter records the error, while the in-app record remains available. Real adapters need a distinct success status and a delivery job/outbox before use.

## Lifecycle and feed

An accepted event creates a notification once; opening marks it read, and dismissing removes it from the active feed without deleting the record. External channel attempts have their own status and do not determine read state. Pages of 10 are newest first; the service caps page size at 100 and scopes all mutations to the recipient. The console host currently has a synchronous, in-memory event publisher, so publication and handling are synchronous **in this implementation**. Future production handling should consume committed events asynchronously through a durable outbox/inbox. JSON read-then-write cannot guarantee concurrency safety; SQL needs unique `(OccurrenceId, RecipientParticipantId)` and `(ParticipantId)` preference keys, an index on `(RecipientParticipantId, DismissedAt, CreatedAt DESC, Id DESC)`, and a stable cursor for high-volume feeds.

## Contracts and extension

`NotificationRequestedV1` carries a source-owned occurrence ID, recipient and actor IDs, kind, subject kind/ID and timestamp. The kind selects a policy and category in Notifications (a small strategy selection); delivery adapters implement `INotificationDelivery`. Keep rendering server-side and version payloads when a new kind needs richer data. Never include private moderation rationales in a notification. New kinds require a recipient rule, privacy check, deduplication key and preference mapping.

## Follow-ups

- Durable outbox/inbox and asynchronous worker; atomic deduplication, retries with bounded backoff, dead-letter inspection and auditing.
- Real email and push adapters, credentials/device registration, provider receipts and real success/failure status.
- Delivery batching or digest and per-Node watch/mute controls for higher comment volume.
- Comment moderation, tag disputes, invitation events when their source workflows exist.
- Cursor paging and SQL persistence with authorization and accessibility for a web/mobile notification center.
