# Notifications Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Create notification from domain event | Internal event flow | Recipient/actor come from the event contract, not an end-user notification command | `Handle` validates required IDs/kind, suppresses self-notification and duplicate occurrence/recipient pairs | Notification tests | Covered as internal flow; event producers remain trust boundary |
| Read notification feed | Recipient | Host supplies authenticated Participant ID as recipient | Repository page is scoped by supplied recipient ID; service itself does not independently authenticate it | Console notification flow | Host identity required |
| Mark notification read | Recipient | Host supplies authenticated Participant ID | `Own(recipientId, notificationId)` verifies stored recipient before mutation | Notification tests | Covered after actor establishment |
| Dismiss notification | Recipient | Host supplies authenticated Participant ID | `Own` rejects a foreign recipient | Foreign-recipient test | Covered |
| Read preferences | Recipient | Host supplies authenticated Participant ID | Repository lookup is keyed by supplied Participant ID; no separate ownership check | Current Console | Host identity required |
| Change preferences | Recipient | Host supplies authenticated Participant ID | Current preference object/repository saves by supplied Participant ID; no independent actor/target comparison | Current Console | **Review:** future workflow should bind preference owner to authenticated actor |
| Deliver email/push | Internal delivery | Triggered from validated notification event/preferences | Delivery adapter receives the already-created notification; external delivery currently simulated | Notification service/tests | Infrastructure boundary |

## Review note

Notification item mutations have a useful recipient check. Feed and preference access rely more heavily on the host supplying the authenticated participant. Preference mutation deserves an explicit owner-bound application workflow before a public API exposes it.
