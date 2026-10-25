# Atlas visual architecture guide

[Documentation home](../../README.md) · [Architecture](../README.md) · [Ownership guide](../APPLICATION-WORKFLOWS.md)

Start with [application wiring and presentation ownership](WIRING.md): Console owns terminal presentation, the REST API translates HTTP requests, SvelteKit owns browser presentation, and Storybook exercises shared frontend components. Both backend hosts reuse application workflows, domain rules and SQL adapters.

The diagrams are editable Mermaid source rendered by GitHub Markdown. Flowcharts show composition and responsibility without empty class compartments. Class diagrams remain where interfaces, inheritance or methods matter; sequences show call order, and process trees show branches/failures. Each view is scoped and links to current source. An arrow's meaning belongs to its diagram: composition, dependency and runtime calls are not interchangeable.

## Choose a route

| Question | Detailed view |
|---|---|
| Who wires the application, and what could another host reuse? | [Application wiring and presentation ownership](WIRING.md) |
| How do Graph and Content create one node, and how do description edits work? | [Node and content UML/process trees](NODES-AND-CONTENT.md) |
| Where do association, vote, comment and notification decisions happen? | [Reactions and notifications](REACTIONS-AND-NOTIFICATIONS.md) |
| Who starts, joins, commits or rolls back a transaction? | [Transaction and event lifecycle](TRANSACTIONS-AND-EVENTS.md) |

```mermaid
flowchart TD
    Q{"What do you want to understand?"}
    Q -->|Structure| W["Application wiring and presentation ownership"]
    Q -->|Node mutation| N["Nodes and descriptions"]
    Q -->|Cross-domain action| R["Reactions and notifications"]
    W --> T["Transaction and event lifecycle"]
    N --> T
    R --> T
```

## Read the design in three layers

The host translates interaction into inputs and supplies verified identity. Application coordinates work spanning domains. Domains enforce their own rules through repository ports; the selected SQL adapters implement those ports and share an EF transaction where a workflow requests one.

Reference validation and authorization are distinct. Checking that a participant is available does not establish that they authored a node. For description mutations, the workflow performs both checks, then Content checks its own document/block rules. A supplied GUID alone does not authenticate a caller.

## A focused PR review

1. Follow startup composition in [WIRING](WIRING.md), including event subscriptions.
2. Trace creation and one description edit in [NODES-AND-CONTENT](NODES-AND-CONTENT.md).
3. Take the failed-vote branch and check notification recipients in [REACTIONS-AND-NOTIFICATIONS](REACTIONS-AND-NOTIFICATIONS.md).
4. Follow a subscriber failure after commit in [TRANSACTIONS-AND-EVENTS](TRANSACTIONS-AND-EVENTS.md).

These views cover the current backend and frontend wiring plus the extraction's transaction boundaries. They are an entry point for further domain UML, not an exhaustive model of every aggregate. Existing [domain architecture documents](../README.md), [workflow documents](../../workflows/README.md) and [blackboards](../../blackboards/README.md) provide the next level of detail.
