# Atlas.Application

The reusable application core: cross-domain workflows, integration adapters, notification recipients, in-memory event dispatch and bounded queries. It references domain projects and Contracts, with no dependency on Console, HTTP, Identity infrastructure, EF or SQL implementations.

NodeWorkflowService, NodeDescriptionService, CommentWorkflowService, ReactionVotingService and ModerationWorkflowService are the operations consumed by hosts. Single-domain operations remain domain services. Hosts provide verified actors, repository implementations, event subscribers and transport-specific output.

See [thin-host ownership and flow](../../docs/architecture/APPLICATION-WORKFLOWS.md) for project direction, transaction/event guarantees and the future API composition recipe.

The [visual architecture guide](../../docs/architecture/diagrams/README.md) connects UML wiring views to node/content, reaction, notification and transaction process diagrams, with source and test links.
