# Atlas requirements

This folder is the starting point for requirements management and traceability in Atlas.

- [REQUIREMENTS.md](REQUIREMENTS.md) defines what Atlas is expected to do and the acceptance criteria for each requirement.
- [TRACEABILITY.md](TRACEABILITY.md) is the Requirements Traceability Matrix (RTM): a broad view from each requirement to implementation, verification, and known gaps.

## Requirement identifiers

Identifiers use the form `AREA-NNN`.

| Prefix | Area |
|---|---|
| `GRA` | Graph node behavior |
| `TYP` | Node types and requested sub-node types |
| `REL` | Parent/child graph relationships |
| `CON` | Content and description documents |
| `PAR` | Participants and profiles |
| `AUT` | Authorization |
| `EVT` | Events and cross-boundary communication |
| `PER` | Persistence and migration |
| `VOT` | Voting |
| `NFR` | Cross-cutting non-functional requirements |

A requirement ID is permanent once assigned. Do not renumber an ID when requirements are reordered, and do not reuse an ID after a requirement is withdrawn. Keep withdrawn records and mark them `Rejected` or `Deferred` so commits, tests, and discussions retain their meaning.

## Requirement wording

Requirements describe externally meaningful behavior and normally use **shall**. Implementation details belong in the RTM, not in the requirement statement. Acceptance criteria should be observable and specific enough to verify.

Capabilities and requirements can overlap, but they answer different questions:

- A capability says what Atlas makes possible.
- A requirement states a behavior or constraint Atlas commits to and can trace to evidence.

## Priority vocabulary

| Priority | Meaning |
|---|---|
| `Must` | Required for the intended core behavior or boundary integrity |
| `Should` | Important, but the current slice can operate temporarily without it |
| `Could` | Useful enhancement with lower delivery priority |

Priority and status are independent. For example, a Must requirement can still be Proposed or Partial.

## RTM status vocabulary

| Status | Meaning |
|---|---|
| `Proposed` | Recorded for discussion; not yet accepted as a commitment |
| `Approved` | Accepted as a requirement; implementation has not started or is not evidenced |
| `Partial` | Some acceptance criteria are implemented, but the requirement is not complete |
| `Implemented` | Code appears to satisfy the requirement, but automated verification is incomplete |
| `Verified` | Automated tests provide evidence for the acceptance criteria |
| `Deferred` | Intentionally postponed while retaining the identifier and rationale |
| `Rejected` | Considered and intentionally not pursued; the identifier is never reused |

`Verified` does not mean the design can never change. It means the current requirement has automated evidence at the level described by its linked tests.

## Forward and backward traceability

Forward traceability answers:

> For this requirement, where is it implemented and how is it verified?

Start with an ID in [REQUIREMENTS.md](REQUIREMENTS.md), then follow its row in [TRACEABILITY.md](TRACEABILITY.md).

Backward traceability answers:

> Why does this code or test exist?

Search the repository for the requirement ID. As the practice matures, include IDs in test documentation, issue descriptions, pull requests, or focused code comments where they add durable context. Avoid scattering IDs into every method merely to satisfy a checklist.

## Maintenance workflow

When adding or changing behavior:

1. Search for related IDs and confirm the behavior is not already represented.
2. Add or amend the requirement statement, rationale, and acceptance criteria.
3. Preserve the existing ID; assign the next unused number only for a genuinely new requirement.
4. Update priority and status honestly.
5. Add implementation and verification links to the RTM.
6. If code exists without tests, use `Implemented`, not `Verified`.
7. If only one host enforces a rule that belongs to a boundary, use `Partial`.
8. Review the links after files move.

Markdown remains intentionally simple and searchable. From the repository root, examples include:

```text
rg "AUT-001"
rg "Status.*Partial" docs/requirements
rg "NodeParentAttachedV1" docs/requirements src tests
```

## Scope of this baseline

This first baseline traces the behavior already visible in the current domain projects and Console prototype. It also records a small number of agreed or clearly anticipated gaps, such as voting, durable messaging, and node-management authorization. It is not intended to capture every future product idea at once.
