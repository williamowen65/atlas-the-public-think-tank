# Moderation boundary — PTT-101

Visual companion: [Moderation Domain blackboard](../blackboards/workflows/Moderation/ModerationDomain.excalidraw)
([editable Excalidraw source](../blackboards/workflows/Moderation/ModerationDomain.excalidraw)).

Moderation owns reports, case status, public reason categories, human decisions, and decision rationales. It references a Node and Participant by ID; Graph and Participants retain ownership of those entities. The Console host applies hidden Node visibility to Discovery and Node views. This works for Nodes without Community membership. Disagreement with an idea alone is not a policy violation.

## First runnable workflow

1. In **Discover nodes**, enter `P` followed by the displayed number (for example `P2`). Supply a short reason and optional explanation. A report stores the reporter ID, Node ID, reported title, time, and a pending status.
2. Sign in with a confirmed, active account assigned the global GlobalModerator or Administrator Identity role. The main menu shows **Review Node reports** for an authorized account; queue and decision operations check current stored role membership again. See [Identity setup](../PTT-94-Identity.md). Public registration grants Member only; assigning a global role is controlled operator work.
3. The queue groups pending reports by Node ID. A moderator opens a group to review all reports for that Node, including previous decisions, and can open the Node to investigate. One reasoned decision dismisses or hides the Node and closes every pending report in that group together. Hiding also requires a public category; the private rationale is stored separately. Each individual report and reviewer ID remains in `ModerationCaseRows`; final decisions cannot be overwritten.
4. Discovery omits hidden Nodes. Console node tables and detail views display `[Hidden by moderator: <public category>]` in place of their title and description while keeping author, votes, reactions, type, and navigation to visible children. The public category is selected independently of the internal decision rationale. A dismissed report has no effect.
5. The author can view the original title and description, edit them, and request review after an edit. An Atlas moderator can open the hidden Node from the review queue and view its current original title and description in a read-only view before deciding whether to restore it. The Node stays hidden until a moderator restores visibility from the review queue. Reports and decisions remain stored for history.
6. A hidden Node cannot receive another report. The Console shows the report action as disabled, and Moderation rejects direct submissions until visibility is restored.

## Ownership and limits

The hide action is Moderation-owned visibility, not a Graph archive or a change to the author's Node. The console host applies it to Discovery and its Node read views. Grouping by target ID lets future report types use the same queue pattern. Moderation cases are stored in SQL Server through `SqlModerationCaseRepository`. A group decision and restoration save each case separately; each save has a transaction, but the full group operation does not share one transaction. The Console establishes the actor through Identity sign-in and disables arbitrary participant switching. Global role membership is supplied through the Moderation authorization port; moderation decisions remain domain-owned.

This slice has a final decision record but does not yet have an append-only event ledger, evidence snapshots beyond the reported title, reporter/author notifications, appeals, account sanctions, or a complete policy taxonomy. Those require follow-up tasks. Community ownership and community moderators remain distinct from Atlas-wide report review. Operational system administrators handle deployments and repair, not routine moderation decisions.

## Future automated signals

A future provider-neutral checker can return a provider and model/version, a category/severity, confidence, and timestamp for triage. It does not decide or enforce. Human moderators can reject false positives and continue reviewing when the provider fails. There is no automated provider in this slice.
