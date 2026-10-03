# Notifications Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Create notification from domain event](../../../src/Atlas.Notifications/Notifications.cs) | Internal event flow | Recipient/actor come from the event contract, not an end-user notification command | `Handle` validates required IDs/kind, suppresses self-notification and duplicate occurrence/recipient pairs | Notification tests | Covered as internal flow; event producers remain trust boundary |
| [Read notification feed](../../../src/Atlas.Notifications/Notifications.cs) | Recipient | `CurrentUserNotifications` derives recipient from `IAuthenticatedActor` | Repository page is scoped by supplied recipient ID; service itself does not independently authenticate it | Console notification flow | Host identity required |
| [Mark notification read](../../../src/Atlas.Notifications/Notifications.cs) | Recipient | `CurrentUserNotifications` derives actor from `IAuthenticatedActor` | `Own(recipientId, notificationId)` verifies stored recipient before mutation | Notification tests | Covered after actor establishment |
| [Dismiss notification](../../../src/Atlas.Notifications/Notifications.cs) | Recipient | `CurrentUserNotifications` derives actor from `IAuthenticatedActor` | `Own` rejects a foreign recipient | Foreign-recipient test | Covered |
| [Read preferences](../../../src/Atlas.Notifications/Notifications.cs) | Recipient | `CurrentUserNotifications` derives actor from `IAuthenticatedActor` | `NotificationService.Preferences` requires actor ID = preference owner ID | Notification ownership tests | Covered after actor establishment |
| [Change preferences](../../../src/Atlas.Notifications/Notifications.cs) | Recipient | `CurrentUserNotifications` derives actor from `IAuthenticatedActor` | `NotificationService.SavePreferences` requires actor ID = preference owner ID | Current Console | Covered — owner check enforced in service |
| [Deliver email/push](../../../src/Atlas.Notifications/Notifications.cs) | Internal delivery | Triggered from validated notification event/preferences | Delivery adapter receives the already-created notification; external delivery currently simulated | Notification service/tests | Infrastructure boundary |

## Review note

Notification host calls now go through `CurrentUserNotifications`, so “my notifications/preferences” operations do not accept a caller-selected recipient at all. That workflow derives the actor from `IAuthenticatedActor` and passes it to `NotificationService`, where recipient/owner checks remain explicit. This demonstrates the intended future API pattern without making the Notifications domain depend on ambient authentication state.
