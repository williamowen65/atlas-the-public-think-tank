# Moderation boundary — PTT-101

Moderation handles reports about inappropriate or unlawful content while preserving room for disagreement. **Disagreeing with an idea is not, by itself, a policy violation.**

Visual reference: [Moderation Domain blackboard](../blackboards/workflows/Moderation/ModerationDomain.excalidraw) (editable Excalidraw source).

## At a glance

| Responsibility | Owner |
|---|---|
| Reports, case status, public reason categories, decisions, rationale | Moderation |
| Node content, lifecycle, and authorship | Graph |
| Public profiles | Participants |
| Applying hidden-content display rules to Console/Discovery | Host and read workflows |
| Global moderator eligibility | Identity authorization port |

Moderation references Nodes and Participants by ID. It works even for Nodes with no Community membership.

## Report-to-review workflow

1. **Report a node.** In Console's **Discover nodes**, enter `P` and the displayed number (for example, `P2`). Supply a short reason and optional explanation. The case records reporter ID, Node ID, reported title, submission time, and pending status.
2. **Open the moderator queue.** Sign in with a confirmed, active account carrying the stored GlobalModerator or Administrator role. The **Review Node reports** menu appears for an authorized account; reads and decisions check current role membership again. [Moderator provisioning](../PTT-94-Identity.md#trusted-moderator-provisioning-ptt-159) is operator-only.
3. **Investigate reports as a group.** Pending reports are grouped by Node ID. A moderator can see all reports and previous decisions, then open the Node.
4. **Decide.** One reasoned decision **dismisses** or **hides** the Node and closes all pending reports in the group. Hide also needs a *public reason category*; the *private rationale* is stored separately. Each report and reviewer stays recorded in `ModerationCaseRows`. Final decisions cannot be overwritten.
5. **Show hidden content safely.** Discovery omits hidden Nodes. Console lists/details replace title and description with `[Hidden by moderator: <public category>]`, while retaining author, type, votes, reactions, and navigation to visible children. A dismissed report does not affect visibility.
6. **Request a second review.** The author can still see the original title/description, edit, and request review after an edit. A moderator can inspect the current original content read-only before restoring visibility. Until restoration, the Node stays hidden. Reports and decisions remain in history.

**While hidden:** Console disables new reporting and Moderation also rejects direct report submissions. Existing votes/reactions can remain visible under the host's display rules, without allowing new hidden-content interactions.

## Web moderation dashboard (PTT-158)

The SvelteKit `/moderation` page provides the global node-report workflow.

### Access control

- Navigation appears only after a successful protected `GET /api/moderation/queue` policy check.
- Direct requests are also guarded by backend authorization. **A hidden navigation link is not a security boundary.**
- Existing `apiRequest` transport handles same-origin cookies and CSRF tokens for decisions.

### Moderator actions

- Review grouped reports and case history
- Dismiss pending reports
- Hide a Node with a public category and separate private rationale
- Restore a Node after the author has edited and requested review
- Open the normal Node page to investigate, without gaining edit ownership

The dashboard distinguishes loading, empty, forbidden, error, and success states.

**Current scope:** Node reports only. Comment-report queue and decision API endpoints are not present. Community moderator roles are a separate future feature.

## Persistence and transaction rules

**Moderation changes visibility, not authorship.** Hiding is not Graph archival and does not transfer Node ownership.

`SqlModerationCaseRepository` stores individual cases in SQL Server. Decisions on grouped reports, review requests, and restoration use a shared operation transaction:

- Either every affected case saves, or the whole operation rolls back.
- Normal SQL locking and constraints handle competing decisions; Moderation does not take the Graph application lock.
- [SQL operation tests](../security/validation/IMPLEMENTATION.md#workflows-and-evidence) cover competing decision paths.
- The Console gets its actor through Identity sign-in, not arbitrary Participant switching.
- Global roles come through the Moderation authorization port; the domain owns the final decision.

Grouping by target ID leaves room for future report types.

## What is not yet implemented?

This slice has final decisions and stored case history. It does **not** yet have:

- A complete append-only audit event ledger
- Full report-time evidence snapshots beyond the reported title
- Reporter/author notifications or appeals
- Account sanctions or a full moderation policy taxonomy
- Comment-report moderation workflow or community moderation roles

See [proposed snapshot and audit migration](../Architecture/moderation-snapshots-migration.md). That document is a **future migration plan**, not a description of deployed storage.

Operational system administrators handle deployments and repair, rather than ordinary content decisions.

## Future automated signals

**Human review stays authoritative.** Automated tools may help triage reports; they must not hide content or make final decisions.

A future provider-neutral checker could report provider/model version, category, severity, confidence, and timestamp. Reviewers must be able to reject false positives and continue working when an external provider is unavailable. No automated checking provider exists in this slice.
