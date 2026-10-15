# Atlas.Application

Reusable coordination across Atlas domains, independent of Console, HTTP and SQL implementations.

- NodeCreationService prepares Content and Graph proposals, then saves them under the node repository's topology boundary. Publish the returned node's recorded events only after successful return.
- NodeDescriptionService authorizes edits through the current node author and delegates typed block operations to Content. Hosts supply verified actor IDs, not identity claims from request bodies.

Domain rules stay in Graph/Content. Hosts supply repositories, transaction capabilities, identity and event dispatch. See [workflow ownership and flow](../../docs/architecture/APPLICATION-WORKFLOWS.md) for guarantees, remaining adapter dependencies and test evidence.
