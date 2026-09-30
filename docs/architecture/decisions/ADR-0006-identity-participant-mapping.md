# ADR-0006: Link Identity accounts to independent Participant profiles

Status: Proposed (PTT-94 PR; pending developer-generated migration and SQL verification)
Date: 2026-09-30
Related requirements: [IDN-001](../../requirements/REQUIREMENTS.md#idn-001), [IDN-002](../../requirements/REQUIREMENTS.md#idn-002)

## Context

Atlas now has SQL Server/EF persistence, a Participants domain, and Atlas-specific authorization workflows. Authentication should use ASP.NET Core Identity without importing credential infrastructure into domain projects. Existing demo Participant IDs and contributions must survive the change.

## Decision

Use IdentityUser<Guid> in infrastructure and share its Id with an independent ParticipantId. Add Identity tables in an `identity` schema using the existing shared AtlasDataContext/migration history. Enforce the one-to-one account/profile relationship with a non-cascading FK. Create the new profile, Identity account, and Member assignment in one SQL transaction.

Use framework account managers/token providers rather than custom password/credential logic. Add baseline Member/Moderator/Administrator policies, checking current database role memberships and active profile eligibility. Keep ownership, community-specific moderation, lifecycle, and report decisions in Atlas domains/application workflows. The HTTP host establishes authenticated actor IDs.

## Consequences

Participants stays independent of Identity/EF. Existing profiles may lack accounts; no public workflow can claim one. One shared context supports atomic account/profile registration today, while the dependency remains explicitly infrastructure-owned. Credentials and role seeds have no default passwords or automatic administrator assignment. The developer scaffolds and commits the migration using EF commands; until then, pending-model/SQL checks intentionally block merge readiness.

The current Console remains a demo harness. HTTP/email delivery, first-administrator provisioning, and durable Data Protection configuration are subsequent host integration, not silently implemented by registering framework services.

## Alternatives considered

- Derive Participant from IdentityUser: couples domain/profile behavior to authentication infrastructure.
- Separate unrelated user/profile IDs: adds a mapping key without a present lifecycle requirement.
- Separate context/database immediately: introduces cross-context transaction/provisioning recovery before Atlas needs independent deployment.
- JSON or SQLite credential storage: conflicts with the adopted SQL Server foundation.

See [Identity architecture](../IDENTITY.md) and [developer setup](../../PTT-94-Identity.md).
