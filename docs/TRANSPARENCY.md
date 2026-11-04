# Atlas transparency and algorithmic accountability

Transparency is a product requirement for Atlas, not only a property of its source-code license. People affected by the platform's rules should be able to inspect, question, test, and propose improvements to the software that shapes their experience.

Atlas uses the GNU Affero General Public License version 3 so the application can remain open to inspection and improvement, including when modified versions are offered over a network. The license is a foundation; the practices below make that transparency usable.

## Commitments

### Public source

Atlas application code, domain rules, database migrations, tests, and public configuration belong in the public repository. Build artifacts must remain traceable to reviewed source.

### Understandable domain specifications

Every bounded context must have a domain README that explains, in plain language:

- what the boundary owns and does not own
- its important rules and calculations
- inputs, outputs, defaults, limits, and failure behavior
- permissions and lifecycle effects
- links to authoritative requirements, decisions, workflows, tests, and code

A reader should not need to reverse-engineer implementation code to learn how an Atlas rule is intended to work.

### Explainable results

When Atlas calculates an important user-facing result, it should provide enough structured explanation for a participant to understand the major reasons for that result.

For example, a Discovery result may identify the ranking policy and the signals that contributed to its position. Explanations must be useful without disclosing another participant's private information or security-sensitive details.

### Versioned decision behavior

Git version control records how source code changes. Algorithm versioning answers a different question: **which named rules and configuration produced this result?**

Important decision behavior should therefore expose a stable policy identifier, such as `discovery-ranking/v1`, together with any public configuration revision needed to interpret it. A material change to inputs, weighting, eligibility, or outcome semantics requires a new version or an explicitly recorded revision.

Atlas does not need a complete product-release versioning system before beginning this practice. A first slice can version one consequential policy—Discovery ranking—and expand the convention when another rule needs the same traceability.

### Public configuration

A public algorithm is not fully understandable if undisclosed weights, thresholds, or feature switches determine its behavior. Configuration that materially changes ranking, eligibility, voting aggregation, or automated recommendations should be versioned and inspectable.

Credentials, secrets, personal information, active security defenses, and vulnerability details awaiting remediation must not be published.

### Auditable decisions

Consequential state changes should preserve appropriate evidence of what happened: actor or decision source, policy or rule, timestamp, reason, relevant version, human override, and resulting action.

Auditability does not mean making all audit records public. Visibility must respect privacy, reporter safety, data-retention rules, and security. Public documentation should still explain what is recorded, who may inspect it, and how decisions may be corrected or appealed.

### Open change process

Atlas uses different tools for different parts of an open change process:

| Surface | Purpose |
|---|---|
| Jira | Authoritative project planning, accepted scope, dependencies, and delivery status |
| GitHub Issues | Public bug reports, focused proposals, and contributor-ready work |
| GitHub Discussions | Early questions, broad ideas, and community conversation |
| Pull requests | Reviewable changes to code and documentation |
| Requirements and ADRs | Committed behavior and durable architectural decisions |

An accepted public proposal should link to its Jira delivery item when one exists. Jira cards that affect public behavior should link back to the relevant GitHub discussion, issue, pull request, or documentation. The two systems should reference one another rather than silently becoming competing backlogs.

### Reproducible verification

Tests should demonstrate important public claims about Atlas behavior. Decision algorithms should include representative fixtures and boundary cases so another person can run the tests, inspect the inputs, and reproduce the expected result.

When behavior changes, the specification, version, tests, and user-facing explanation must change together.

## Where transparency lives

| Question | Primary location |
|---|---|
| What is Atlas trying to do? | Repository README and requirements |
| Which boundary owns a rule? | Architecture documentation and the boundary README |
| How does an algorithm work? | Boundary README linked to its authoritative specification |
| Why was the design chosen? | Architecture decision record |
| How does a cross-boundary workflow behave? | Workflow documentation |
| How is the behavior verified? | Tests and requirements traceability |
| What changed? | Git history, pull requests, and the algorithm's version history |
| How can someone question or improve it? | GitHub Discussions, GitHub Issues, and the contribution guide |

The [documentation index](README.md) is the entry point for requirements, architecture, workflows, testing guidance, the glossary, and blackboards.

## Current state

Atlas already provides public source, version control, requirements traceability, architectural decision records, automated tests, and several boundary READMEs. Moderation records reasoned decisions and audit history, and Discovery has an explicit ranked-query boundary.

The transparency surface is not complete:

- Content, Voting, Comments, Discovery, Moderation, and Contracts need boundary READMEs.
- Voting needs a plain-language specification of rating and reaction calculations.
- Discovery needs a published ranking specification, public policy/configuration version, and result explanations.
- A consistent algorithm-version identifier and change-history convention has not been adopted.
- Contribution guidance exists in the root README, but contributor governance and the relationship between Jira and GitHub need fuller documentation.
- Open GitHub issues from the legacy application need triage so contributors can distinguish current work from historical work.

These are tracked as implementation and documentation work. This document states the direction; it does not claim that every commitment is already satisfied.

## Review questions

Changes to consequential Atlas behavior should answer:

1. Which boundary owns the rule?
2. Where is the rule explained in plain language?
3. Which inputs and configuration affect the outcome?
4. Can an affected participant understand the major reasons for the result?
5. Does the change require a new policy or algorithm version?
6. What audit evidence is retained, and who may see it?
7. Which tests reproduce the behavior and its edge cases?
8. Which requirement, ADR, Jira item, and public discussion or issue are linked?
9. Does transparency expose private, abusive, or security-sensitive information?
