# Contributing to Atlas

Thank you for helping build Atlas. Contributions may include code, tests, documentation, architecture, accessibility, product design, bug reports, and focused proposals.

Atlas is still under active development. Please use the process below so contributors can participate without creating a second delivery backlog or spending time on work that is not ready to be implemented.

## Where work belongs

| Place | Use it for |
|---|---|
| [GitHub Discussions](https://github.com/williamowen65/atlas-the-public-think-tank/discussions) | Early questions, broad ideas, and topics that need exploration |
| [GitHub Issues](https://github.com/williamowen65/atlas-the-public-think-tank/issues) | Public bug reports, focused proposals, and contribution opportunities |
| Jira | Work that Atlas has accepted, prioritized, and committed to deliver |
| Pull requests | Reviewable changes to code, tests, and documentation |
| Requirements and ADRs | Committed behavior and durable architectural decisions |

GitHub Issues and Jira are intentionally separate. They are not synchronized backlogs.

When a GitHub proposal becomes committed work, a maintainer may create a Jira item and cross-link the two records. Jira tracks delivery from that point; the GitHub issue remains the public context and contribution record. A Jira item affecting public behavior should link to the relevant issue or discussion, pull request, and durable documentation when those records exist.

## Contribution path

1. **Start in the right place.** Use a Discussion for an early or wide-ranging idea. Use an issue template for a reproducible bug or focused proposal.
2. **Let the direction become clear.** A maintainer may ask questions, narrow the scope, or connect the proposal to existing requirements and architecture.
3. **Look for `contribution-ready`.** This label means the direction and scope are suitable for implementation. It is not a promise that every submitted implementation will merge, but it prevents contributors from unknowingly building against an unaccepted direction.
4. **Create a focused branch from `main`.** Keep unrelated cleanup out of the change.
5. **Implement through the owning boundary.** Preserve bounded-context ownership. When a workflow crosses boundaries, use public contracts or application coordination instead of reaching into another boundary's internal state.
6. **Verify the behavior.** Add or update automated tests, run the relevant test projects, and test the Console workflow when it exposes the changed behavior.
7. **Update durable documentation.** Change requirements, traceability, ADRs, domain READMEs, diagrams, or other documentation when the behavior or architecture changes.
8. **Open a pull request.** Complete the pull-request template, link the public issue and any Jira item, and explain how the change was verified.
9. **Address review.** Maintainers review behavior, architecture, tests, documentation, security, accessibility, and scope. Important design decisions should be recorded in the repository rather than left only in review comments.
10. **Merge and close.** A merged pull request closes the public implementation record. Jira, when present, continues to track committed delivery status; its status does not need to mirror the GitHub issue.

## Understanding the labels

| Label | Meaning |
|---|---|
| `needs-discussion` | The problem or direction needs more public discussion before implementation |
| `contribution-ready` | The issue has a clear enough direction and scope for someone to begin work |
| `good first issue` | A contribution-ready issue that is approachable for a newer contributor |
| `help wanted` | Maintainers would especially welcome outside help |
| `blocked` | Work cannot proceed until a dependency or decision is resolved |
| `legacy` | Historical work from the earlier Atlas implementation |
| `bug`, `enhancement`, `documentation` | The kind of contribution being discussed |

Only maintainers should apply `contribution-ready`, because it communicates that Atlas is prepared to review implementation in that direction. Contributors are welcome to comment on or propose refinements to any open issue.

## Development and verification

Atlas currently targets [.NET 10](https://dotnet.microsoft.com/).

```bash
git clone https://github.com/williamowen65/atlas-the-public-think-tank.git
cd atlas-the-public-think-tank
dotnet restore Atlas.sln
dotnet test Atlas.sln
dotnet run --project src/Atlas.Console/Atlas.Console.csproj
```

Before opening a pull request:

- build the solution and run the relevant tests;
- add tests for new or changed behavior;
- confirm existing behavior still passes;
- exercise the changed workflow in the Console when applicable;
- update documentation and traceability where behavior changes;
- avoid committing secrets, credentials, generated build output, or unrelated formatting changes.

If you cannot run part of the verification, say exactly what was not run and why in the pull request.

## Review and acceptance

A `contribution-ready` issue approves a direction for development; it does not automatically approve a particular implementation. A pull request is accepted only after review confirms that it:

- solves the agreed problem without unnecessary scope;
- respects bounded-context ownership and public contracts;
- includes appropriate tests and verification;
- updates affected documentation;
- does not introduce known security, privacy, fairness, or accessibility concerns; and
- passes required automated checks.

Substantial changes to public behavior or architecture may require an updated requirement or ADR before merge.

## Contribution licensing

Atlas is licensed under the [GNU Affero General Public License version 3](LICENSE).

By submitting a contribution, you confirm that you created it or otherwise have the right to submit it, and you agree that it may be distributed under the project's AGPL-3.0 license. Do not contribute code, media, data, or other material that you do not have permission to license this way.

Atlas does **not currently require** a separate Contributor License Agreement (CLA) or a Developer Certificate of Origin (DCO) `Signed-off-by` line. This keeps the contribution process approachable while the project is young. The policy may be revisited with appropriate legal review if Atlas's governance, organizational structure, or distribution needs change.

## Questions

If you are unsure where to begin, start a [GitHub Discussion](https://github.com/williamowen65/atlas-the-public-think-tank/discussions). Asking before undertaking a large change is welcome.
