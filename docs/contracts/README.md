# Atlas integration contracts

Atlas.Contracts is the single source of truth for payload shapes exchanged across boundary communication. This catalog explains ownership and compatibility without duplicating the C# record definitions.

## Current Graph V1 contracts

| Contract | Producer | Known consumer | Trigger |
|---|---|---|---|
| `NodeCreatedV1` | Graph | Console-owned Content observer | Successful node creation |
| `NodeArchivedV1` | Graph | Console-owned Content observer | Active node archived |
| `NodeRestoredV1` | Graph | Available for subscribers | Archived node restored |
| `NodeParentAttachedV1` | Graph | No required consumer yet | Parent attached |
| `NodeParentDetachedV1` | Graph | No required consumer yet | Parent detached |

Authoritative definitions: [NodeLifecycleEvents.cs](../../src/Atlas.Contracts/Graph/V1/NodeLifecycleEvents.cs)

## Compatibility rules

- A published contract name includes its major version.
- Existing fields are not renamed, removed, or reinterpreted within a version.
- A breaking change creates a new version such as `NodeCreatedV2`.
- Producers may need to publish old and new versions during migration.
- Consumers should ignore fields they do not understand when serialization permits it.
- Contracts contain identifiers and communication data, never domain entities or methods.
- Contract evolution should receive serialization compatibility tests.
- Event identity and causation/correlation metadata should be considered before durable messaging.

## Ownership

The producing capability owns the semantic meaning of its event. Atlas.Contracts owns the shared payload definition used by producers and consumers. A consumer owns its reaction and may ignore an event entirely.

The current in-memory publisher is a host implementation detail. Moving to a broker should not change what a published contract means.
