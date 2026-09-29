# Atlas documentation

This directory records what Atlas must do, how its boundaries fit together, why important decisions were made, and how the implementation is verified.

| Area | Purpose |
|---|---|
| [Requirements](requirements/README.md) | Requirement catalog, stable IDs, acceptance criteria, and traceability |
| [Architecture](architecture/README.md) | Boundary map, data ownership, and architectural decisions |
| [Contracts](contracts/README.md) | Published communication shapes and compatibility expectations |
| [Workflows](workflows/README.md) | Multi-boundary behavior, sequencing, and failure paths |
| [Testing](testing/TEST-STRATEGY.md) | Test levels, responsibilities, and verification standards |
| [Glossary](glossary/GLOSSARY.md) | Shared Atlas domain language |
| [Blackboards](blackboards/README.md) | Exploratory architecture visuals linked to authoritative documentation |
| [Legacy SQL](sql/) | Reference material from the earlier application |

## How the documents connect

A useful review path is:

1. Start with a requirement and its acceptance criteria.
2. Use the RTM to find implementation and verification evidence.
3. Read the relevant ADR to understand why the design was chosen.
4. Check the context map and data-ownership guide for boundary responsibilities.
5. Follow a workflow document when behavior crosses multiple boundaries.
6. Use the glossary when a domain term could be interpreted more than one way.

Documents should link rather than repeat authoritative definitions. Requirements own committed behavior, ADRs own decisions, contracts own payload descriptions, and code remains the executable implementation.

## Code documentation

- Give classes, records, interfaces, and methods concise XML summaries so their
  responsibilities appear in IntelliSense.
- Describe the boundary responsibility, lifecycle transition, workflow step,
  or invariant a member supports rather than restating its signature.
- State the protected behavior in test documentation so the suite also serves
  as a readable inventory of expectations.
- Keep comments synchronized with behavior and link to authoritative documents
  instead of copying detailed rules into code.

## Mobile-friendly tables

Documentation should remain usable from a phone:

- Use a conventional table when it has two or three concise columns and its
  value comes from scanning or comparing rows.
- When one record has several descriptive attributes, give that record its own
  heading and use a two-column `Attribute | Current state` table.
- Keep the same attribute order across related record tables so they still act
  as one logical matrix.
- Use short phrases or controlled `<br>` breaks inside long cells.
- Do not rely on HTML width attributes or custom CSS; GitHub's mobile renderer
  may ignore them.
- Preserve a genuinely wide matrix only when side-by-side comparison is more
  important than mobile readability, such as a requirements traceability
  matrix.
