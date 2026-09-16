# ADR-0005: Enforce current-vote uniqueness at the persistence boundary

Status: Accepted  
Date: 2026-09-16  
Related requirements: [VOT-002](../../requirements/REQUIREMENTS.md#vot-002), [VOT-004](../../requirements/REQUIREMENTS.md#vot-004), [VOT-010](../../requirements/REQUIREMENTS.md#vot-010)

## Context

Voting permits a participant to hold at most one current vote for a target. A lookup followed by a separate save can violate that invariant when concurrent requests both observe that no vote exists.

A lock owned by CastVote protects only callers sharing one application-service instance. Static or instance locks remain local to one process and cannot coordinate horizontally scaled Voting service instances.

Voting owns the uniqueness rule and must be testable without delegating the business decision to another bounded context. Persistence technology still determines how an atomic mutation is implemented.

## Decision

IVoteRepository defines SetCurrentVote as one atomic participant-target operation. CastVote validates mutation policy and delegates the complete create-or-change operation through that port instead of composing a separate lookup and save.

Repository implementations must treat participant ID, target type, and target ID as the uniqueness key for the complete read-modify-write operation.

- InMemoryVoteRepository uses an instance gate around its shared collection.
- JsonVoteRepository shares an in-process gate between adapter instances addressing the same normalized file path and locks the complete file read-modify-write sequence.
- A production transactional repository must use storage-level atomicity and a unique constraint over participant ID, target type, and target ID.

The production database constraint is defense in depth and the authoritative cross-process safeguard. It enforces a Voting-owned invariant; it does not relocate ownership of the rule outside Voting.

## Consequences

- CastVote no longer relies on its own object lifetime for concurrency correctness.
- Real in-memory and JSON adapters can be tested against the atomic repository contract.
- Separate JSON adapter instances in one process coordinate when they address the same file.
- JSON remains prototype infrastructure and does not claim cross-process or cross-server safety.
- Horizontally scaled Voting requires shared transactional persistence with participant-target uniqueness.
- Each future persistence adapter must demonstrate that SetCurrentVote satisfies the Voting contract.
- Low-level Save remains available for persistence support but is not used by the CastVote workflow.

## Alternatives considered

A lock on each CastVote instance was rejected as the final design because separate application-service instances do not share it.

A static application lock was rejected because it serializes unrelated votes, remains process-local, and does not protect multiple servers.

A universal repository base class was rejected because one inherited C# lock cannot provide the different atomicity mechanisms required by JSON files, relational databases, and remote stores. The interface defines the required behavior while concrete adapter tests verify each implementation.

A distributed lock service was deferred because transactional shared persistence and a uniqueness constraint provide a simpler authoritative enforcement point for this invariant.
