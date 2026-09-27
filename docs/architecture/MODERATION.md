# Moderation boundary — PTT-101

Moderation owns reports, case status, human decisions, and decision rationales. It references a Node and Participant by ID; Graph and Participants retain ownership of those entities. The Console host composes Moderation with Discovery so that a sustained report excludes the Node from public results. This works for Nodes without Community membership. Disagreement with an idea alone is not a policy violation.

## First runnable workflow

1. In **Discover nodes**, enter `P` followed by the displayed number (for example `P2`). Supply a short reason and optional explanation. A report stores the reporter ID, Node ID, reported title, time, and a pending status.
2. For local development, open **Manage User Secrets** on the `Atlas.Console` project in Visual Studio and set `{"ATLAS_MODERATOR_PARTICIPANT_IDS":["11111111-1111-4111-8111-111111111111"]}` (or use other GUIDs from `data/participants.json`). The project uses the existing checked-in `UserSecretsId`. For other environments, set `ATLAS_MODERATOR_PARTICIPANT_IDS` to a comma-separated string of participant GUIDs; environment configuration overrides the local User Secrets value. Restart the Console after changing configuration. The menu shows **Review Node reports** for a configured moderator. The service checks authorization again for both queue and decision operations.
3. A moderator selects a case and records a rationale when dismissing or excluding the Node from Discovery. A final decision cannot be overwritten. The report and reviewer ID are stored in `data/moderation-cases.json`.
4. Discovery recomputes the exclusion set from actioned cases on each query. Its filtering rule also applies if `IncludeArchived` is true. A dismissed report has no effect.

## Ownership and limits

The exclusion is Discovery-owned visibility, not a Graph archive or a change to the author's Node. Direct ID lookup and other Node navigation paths remain available. Later integration must define consistent visibility across every public surface and how an exclusion is reversed on appeal. Moderation cases are stored in JSON for the Console prototype; the file is not transactional across processes. The current Console lets anyone select a Participant locally, so its participant switcher does not establish production-grade identity. Configure moderator IDs only for local development and integrate authenticated identity/roles before exposing this workflow through an API.

This slice has a final decision record but does not yet have an append-only event ledger, evidence snapshots beyond the reported title, reporter/author notifications, appeals, account sanctions, or a complete policy taxonomy. Those require follow-up tasks. Community ownership and community moderators remain distinct from Atlas-wide report review. Operational system administrators handle deployments and repair, not routine moderation decisions.

## Future automated signals

A future provider-neutral checker can return a provider and model/version, a category/severity, confidence, and timestamp for triage. It does not decide or enforce. Human moderators can reject false positives and continue reviewing when the provider fails. There is no automated provider in this slice.
