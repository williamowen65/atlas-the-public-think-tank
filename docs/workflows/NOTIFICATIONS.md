# Notifications workflow

```mermaid
flowchart TD
  A["Comment or moderation action"] --> B["Source saves its change"]
  B --> C["Host publishes NotificationRequestedV1"]
  C --> D["Notifications checks duplicate, self, preferences"]
  D --> E["Store recipient record"]
  E --> F["In-app feed"]
  E --> G["Optional simulated delivery adapters"]
  G --> H["Record attempt or failure"]
```

The console dispatch is currently synchronous. The intended durable event delivery is a follow-up; see [boundary and requirements](../architecture/NOTIFICATIONS.md).

[Open the editable Notifications Blackboard](../blackboards/workflows/Notifications/NotificationsDomain.excalidraw) for the current console workflow and ownership boundaries.

[Open the event bus Blackboard](../blackboards/workflows/System/InMemoryEventBus.excalidraw) to see how the console registers and dispatches notification requests.
