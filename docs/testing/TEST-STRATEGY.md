# Atlas test strategy

This strategy defines the evidence expected at each level. It complements project-specific coverage documents such as [Graph test coverage](../../tests/Atlas.Graph.Tests/TEST-COVERAGE.md).

## Test levels

| Level | Primary question | Examples |
|---|---|---|
| Domain unit | Does one model enforce its invariants and transitions? | Node title validation, parent attachment, profile state |
| Application use-case | Does a boundary coordinate behavior and policy correctly? | Only an owner may update a profile |
| Repository contract | Does every adapter preserve the repository contract? | Save/get/reconstitute for JSON and future database adapters |
| Serialization contract | Can producers and consumers exchange compatible payloads? | `NodeCreatedV1` JSON shape and required fields |
| Boundary integration | Do real boundary APIs complete a workflow together? | Create Document, create Node, publish event |
| Host/end-to-end | Can a user complete the workflow through the running host? | Console creates and navigates to a sub-node |
| Performance/load | Does deployed infrastructure meet measurable targets? | Browse latency and concurrent vote submissions |

## RTM verification rule

A requirement becomes **Verified** only when linked automated tests provide evidence for its acceptance criteria. A working Console demonstration is valuable but normally supports **Implemented**, not **Verified**.

## Test organization

- Keep tests grouped by the behavior they explain, even when that creates several focused test files for one aggregate.
- Prefer short Arrange–Act–Assert tests with descriptive method names.
- Test public application behavior and important domain invariants.
- Use `InternalsVisibleTo` deliberately when direct domain tests are valuable but mutation APIs should remain hidden from external hosts.
- Do not test private implementation details merely to raise a coverage percentage.

## Next high-value tests

1. Repository contract tests for all four JSON adapters.
2. Console-independent cycle-prevention tests once the rule moves behind Graph.
3. Contract serialization tests for every V1 integration event.
4. Boundary integration test for Document → Node → event publication.
5. Authorization tests for future protected Graph mutations.
6. Compatibility tests for legacy JSON migration.

## Load-testing entry point

Load testing should begin after Atlas has a network API and a realistic database because the current interactive Console and JSON files are not representative deployment surfaces. Before running load tests, define a workload and target such as p95 latency, error rate, concurrent users, and data volume. Performance tests should not replace correctness or integration tests.
