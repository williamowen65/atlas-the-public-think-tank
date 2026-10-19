# Security

This area contains Atlas's durable security review and architecture documentation.

The structure was established by PTT-97, the pre-UI application security baseline. It is organized by security concern rather than Jira issue number so the documentation remains useful after the review is complete.

## Principles

- Security does not depend on the Console or a future UI behaving correctly.
- ASP.NET Core Identity establishes the authenticated account and its Atlas Participant identity.
- Atlas application and domain rules determine what that participant may read or change.
- Existing enforcement and tests are reused as evidence rather than duplicated.
- HTTP/API/browser controls are documented before the host exists, but speculative middleware and endpoint protection are not implemented early.
- Findings should be traceable from expectation to enforcement, automated evidence, and follow-up work.

## Planned review areas

| Area | PTT-97 child | Purpose |
|---|---|---|
| `authorization/` | PTT-120 | Who may read or change each resource or operation |
| [validation/](validation/README.md) | PTT-121 | Input and domain-boundary validation |
| `resource-limits/` | PTT-122 | Bounds on user-controlled values and structures |
| [negative-paths/](negative-paths/README.md) | PTT-123 | Security-relevant failure and rejection behavior |
| [abuse-prevention/](abuse-prevention/README.md) | PTT-124 | Rate limiting and repeated-request abuse responsibilities |
| `api-browser/` | PTT-125 | Future HTTP, API, and browser security controls |
| [observability/](observability/README.md) | PTT-126 | Security logging, audit, and observability requirements |

Only the review area currently being worked should be created. Empty placeholder folders/files are intentionally avoided.

PTT-123 and PTT-124 share one [pre-API negative-path review](negative-paths/README.md) and [abuse responsibility inventory](abuse-prevention/README.md). The node quota is retained, and Voting action, Community creation and report submission quotas are added. Communities allow one creation per participant per rolling 24 hours; membership/association churn remains outside that budget. Reports allow three submissions per participant per rolling hour, with retry/support guidance; moderator decisions are outside the submission budget. The new ledger tables require developer-generated migrations. API middleware and paid automated screening are not implemented in this slice.
