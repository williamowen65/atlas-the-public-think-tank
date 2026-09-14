# Atlas requirements traceability matrix

This RTM provides the broad implementation and verification view. Follow a requirement ID to its stable, detailed section in [REQUIREMENTS.md](REQUIREMENTS.md). Status terms are defined in the [requirements guide](README.md#rtm-status-vocabulary).

| Requirement | Summary | Priority | Status | Implementation evidence | Verification evidence |
|---|---|---:|---|---|---|
| [GRA-001](REQUIREMENTS.md#gra-001) | Nodes receive stable unique identifiers | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeId.cs](../../src/Atlas.Graph/Nodes/NodeId.cs) | [NodeConstructionTests.cs](../../tests/Atlas.Graph.Tests/NodeConstructionTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [GRA-002](REQUIREMENTS.md#gra-002) | Nodes have validated titles | Must | **Verified** | [NodeTitle.cs](../../src/Atlas.Graph/Nodes/NodeTitle.cs) | [NodeValueObjectTests.cs](../../tests/Atlas.Graph.Tests/NodeValueObjectTests.cs) |
| [GRA-003](REQUIREMENTS.md#gra-003) | Nodes support archive and restore lifecycle states | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeStatus.cs](../../src/Atlas.Graph/Nodes/NodeStatus.cs) | [NodeLifecycleBehaviorTests.cs](../../tests/Atlas.Graph.Tests/NodeLifecycleBehaviorTests.cs) |
| [GRA-004](REQUIREMENTS.md#gra-004) | Nodes retain their author reference | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeAuthorId.cs](../../src/Atlas.Graph/Nodes/NodeAuthorId.cs) | [NodeConstructionTests.cs](../../tests/Atlas.Graph.Tests/NodeConstructionTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [GRA-005](REQUIREMENTS.md#gra-005) | Nodes reference descriptions owned by Content | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeDescriptionId.cs](../../src/Atlas.Graph/Nodes/NodeDescriptionId.cs) | [NodeConstructionTests.cs](../../tests/Atlas.Graph.Tests/NodeConstructionTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [TYP-001](REQUIREMENTS.md#typ-001) | Node types use stable GUID identifiers | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeTypeId.cs](../../src/Atlas.Graph/Nodes/NodeTypes/NodeTypeId.cs) | [NodeMutationTests.cs](../../tests/Atlas.Graph.Tests/NodeMutationTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [TYP-002](REQUIREMENTS.md#typ-002) | Users can create globally reusable node types | Must | **Implemented** | [NodeTypeDefinition.cs](../../src/Atlas.Graph/Nodes/NodeTypes/NodeTypeDefinition.cs)<br>[JsonNodeTypeRepository.cs](../../src/Atlas.Console/Storage/JsonNodeTypeRepository.cs)<br>[ConsoleUi.cs](../../src/Atlas.Console/ConsoleUi.cs) | — |
| [TYP-003](REQUIREMENTS.md#typ-003) | Node authors declare requested sub-node types | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[RequestedSubNodeType.cs](../../src/Atlas.Graph/Nodes/RequestedSubNodeType.cs) | [NodeMutationTests.cs](../../tests/Atlas.Graph.Tests/NodeMutationTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [TYP-004](REQUIREMENTS.md#typ-004) | Comment is requested by default | Should | **Partial** | [ConsoleUi.cs](../../src/Atlas.Console/ConsoleUi.cs)<br>[NodeCreationWorkflow.cs](../../src/Atlas.Console/NodeCreationWorkflow.cs) | — |
| [TYP-005](REQUIREMENTS.md#typ-005) | Type labels support controlled pluralization | Should | **Verified** | [NodeTypeDefinition.cs](../../src/Atlas.Graph/Nodes/NodeTypes/NodeTypeDefinition.cs)<br>[NodeDisplay.cs](../../src/Atlas.Console/NodeDisplay.cs)<br>[ConsoleUi.cs](../../src/Atlas.Console/ConsoleUi.cs) | [NodeTypePluralizationTests.cs](../../tests/Atlas.Graph.Tests/NodeTypePluralizationTests.cs) |
| [REL-001](REQUIREMENTS.md#rel-001) | Nodes support zero or multiple parents | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs) | [NodeParentBehaviorTests.cs](../../tests/Atlas.Graph.Tests/NodeParentBehaviorTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [REL-002](REQUIREMENTS.md#rel-002) | Invalid direct parent links are rejected | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs) | [NodeParentBehaviorTests.cs](../../tests/Atlas.Graph.Tests/NodeParentBehaviorTests.cs) |
| [REL-003](REQUIREMENTS.md#rel-003) | Circular parent relationships are prevented | Must | **Partial** | [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs) | — |
| [REL-004](REQUIREMENTS.md#rel-004) | Parent attachments and detachments emit facts | Should | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeLifecycleEvents.cs](../../src/Atlas.Contracts/Graph/V1/NodeLifecycleEvents.cs) | [NodeParentBehaviorTests.cs](../../tests/Atlas.Graph.Tests/NodeParentBehaviorTests.cs) |
| [CON-001](REQUIREMENTS.md#con-001) | Content owns documents and document identifiers | Must | **Implemented** | [Document.cs](../../src/Atlas.Content/Documents/Document.cs)<br>[DocumentId.cs](../../src/Atlas.Content/Documents/DocumentId.cs) | — |
| [CON-002](REQUIREMENTS.md#con-002) | Description documents persist separately from nodes | Must | **Implemented** | [JsonDocumentRepository.cs](../../src/Atlas.Console/Storage/JsonDocumentRepository.cs)<br>[JsonNodeRepository.cs](../../src/Atlas.Console/Storage/JsonNodeRepository.cs) | — |
| [PAR-001](REQUIREMENTS.md#par-001) | Participants have validated public profiles | Must | **Verified** | [Participant.cs](../../src/Atlas.Participants/Participants/Participant.cs)<br>[ParticipantId.cs](../../src/Atlas.Participants/Participants/ParticipantId.cs) | [ParticipantTests.cs](../../tests/Atlas.Participants.Tests/ParticipantTests.cs) |
| [PAR-002](REQUIREMENTS.md#par-002) | Users can browse participant profiles and contributions | Should | **Implemented** | [ParticipantCommands.cs](../../src/Atlas.Console/Participants/ParticipantCommands.cs)<br>[ParticipantDisplay.cs](../../src/Atlas.Console/Participants/ParticipantDisplay.cs) | — |
| [AUT-001](REQUIREMENTS.md#aut-001) | Participants may edit only their own profiles | Must | **Verified** | [UpdateParticipantProfile.cs](../../src/Atlas.Participants/Profiles/UpdateParticipantProfile.cs)<br>[Participant.cs](../../src/Atlas.Participants/Participants/Participant.cs)<br>[AssemblyInfo.cs](../../src/Atlas.Participants/Properties/AssemblyInfo.cs) | [UpdateParticipantProfileTests.cs](../../tests/Atlas.Participants.Tests/UpdateParticipantProfileTests.cs) |
| [AUT-002](REQUIREMENTS.md#aut-002) | Node management is limited to authorized actors | Must | **Proposed** | — | — |
| [EVT-001](REQUIREMENTS.md#evt-001) | Node creation records a versioned integration fact | Must | **Verified** | [Node.cs](../../src/Atlas.Graph/Node.cs)<br>[NodeLifecycleEvents.cs](../../src/Atlas.Contracts/Graph/V1/NodeLifecycleEvents.cs) | [NodeConstructionTests.cs](../../tests/Atlas.Graph.Tests/NodeConstructionTests.cs)<br>[NodeReconstitutionTests.cs](../../tests/Atlas.Graph.Tests/NodeReconstitutionTests.cs) |
| [EVT-002](REQUIREMENTS.md#evt-002) | The host broadcasts events to interested subscribers | Should | **Implemented** | [InMemoryEventPublisher.cs](../../src/Atlas.Console/Eventing/InMemoryEventPublisher.cs)<br>[Program.cs](../../src/Atlas.Console/Program.cs)<br>[ObserveNodeLifecycleInContent.cs](../../src/Atlas.Console/Content/ObserveNodeLifecycleInContent.cs) | — |
| [PER-001](REQUIREMENTS.md#per-001) | Prototype boundary data is stored in separate files | Must | **Implemented** | [JsonNodeRepository.cs](../../src/Atlas.Console/Storage/JsonNodeRepository.cs)<br>[JsonNodeTypeRepository.cs](../../src/Atlas.Console/Storage/JsonNodeTypeRepository.cs)<br>[JsonDocumentRepository.cs](../../src/Atlas.Console/Storage/JsonDocumentRepository.cs)<br>[JsonParticipantRepository.cs](../../src/Atlas.Console/Storage/JsonParticipantRepository.cs) | — |
| [PER-002](REQUIREMENTS.md#per-002) | Legacy node records migrate without losing descriptions | Should | **Implemented** | [JsonNodeRepository.cs](../../src/Atlas.Console/Storage/JsonNodeRepository.cs) | — |
| [VOT-001](REQUIREMENTS.md#vot-001) | Node views report vote totals and averages | Must | **Verified** | [GetVoteSummary.cs](../../src/Atlas.Voting/GetVoteSummary.cs), [VoteSummary.cs](../../src/Atlas.Voting/VoteSummary.cs), [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs), [ConsoleApplication.cs](../../src/Atlas.Console/ConsoleApplication.cs), [NodeDisplay.cs](../../src/Atlas.Console/NodeDisplay.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs): empty summary, target filtering, arithmetic mean, and current-participant vote tests; Console views manually verified |
| [VOT-002](REQUIREMENTS.md#vot-002) | Voting is the single authority for vote behavior | Must | **Partial** | [Vote.cs](../../src/Atlas.Voting/Model/Vote.cs), [CastVote.cs](../../src/Atlas.Voting/CastVote.cs), [GetVoteSummary.cs](../../src/Atlas.Voting/GetVoteSummary.cs), [IVoteRepository.cs](../../src/Atlas.Voting/Data/IVoteRepository.cs), [Voting boundary](../architecture/VOTING.md) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs); future target policies and lifecycle operations remain incomplete |
| [VOT-003](REQUIREMENTS.md#vot-003) | Only authenticated participants may vote | Must | **Partial** | [Vote.cs](../../src/Atlas.Voting/Model/Vote.cs), [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs) verifies participant attribution; authentication remains deferred |
| [VOT-004](REQUIREMENTS.md#vot-004) | One current vote per participant and target | Must | **Partial** | [CastVote.cs](../../src/Atlas.Voting/CastVote.cs), [InMemoryVoteRepository.cs](../../src/Atlas.Voting/Data/InMemoryVoteRepository.cs), [JsonVoteRepository.cs](../../src/Atlas.Console/Storage/JsonVoteRepository.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs): `CastVote_WhenParticipantAlreadyVoted_ThrowsException`, `CastVote_WithDifferentParticipants_SavesBothVotes`; concurrent uniqueness remains unverified |
| [VOT-005](REQUIREMENTS.md#vot-005) | Participants can change or undo their votes | Must | **Approved** | Not implemented; [removal decision](../architecture/VOTING.md#removal-and-history) | — |
| [VOT-006](REQUIREMENTS.md#vot-006) | Node votes use a 0–10 general rating | Must | **Partial** | [NodeRating.cs](../../src/Atlas.Voting/Model/Votes/VoteValue/NodeRating.cs), [Vote.cs](../../src/Atlas.Voting/Model/Vote.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs): inclusive range, out-of-range rejection, Node target policy, and immutable value tests; change and undo remain unimplemented |
| [VOT-007](REQUIREMENTS.md#vot-007) | NodeTag votes measure node-specific applicability | Must | **Approved** | Not implemented; [NodeTag applicability policy](../architecture/VOTING.md#nodetag-applicability-policy) | — |
| [VOT-008](REQUIREMENTS.md#vot-008) | Current Node votes are publicly auditable | Must | **Partial** | [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs), [NodeDisplay.cs](../../src/Atlas.Console/NodeDisplay.cs), [Vote.cs](../../src/Atlas.Voting/Model/Vote.cs) | Console voter-list and participant-profile navigation manually verified; undo behavior is not implemented |
| [VOT-009](REQUIREMENTS.md#vot-009) | Archived targets reject voting interaction | Must | **Partial** | [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs) performs the current host-level Node status check; [target availability decision](../architecture/VOTING.md#authorization-and-target-availability) | Manual Console verification only; boundary-level eligibility and NodeTag behavior remain unimplemented |
| [VOT-010](REQUIREMENTS.md#vot-010) | Aggregates remain correct under concurrent voting | Must | **Partial** | [NodeCommands.cs](../../src/Atlas.Console/NodeCommands.cs), [NodeVoteSummary.cs](../../src/Atlas.Console/NodeVoteSummary.cs), [NodeDisplay.cs](../../src/Atlas.Console/NodeDisplay.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs) verifies two independent sequential votes; concurrency, NodeTag totals, and real-time consistency remain unverified |
| [VOT-011](REQUIREMENTS.md#vot-011) | Votes retain audit and lifecycle fields | Must | **Partial** | [Vote.cs](../../src/Atlas.Voting/Model/Vote.cs), [StoredVote.cs](../../src/Atlas.Console/Storage/StoredVote.cs), [JsonVoteRepository.cs](../../src/Atlas.Console/Storage/JsonVoteRepository.cs) | [VotingTests.cs](../../tests/Atlas.Voting.Tests/VotingTests.cs) verifies value and participant-target behavior; JSON reconstitution and timestamp evidence remain to be expanded |
| [NFR-001](REQUIREMENTS.md#nfr-001) | Boundaries communicate through identifiers and contracts | Must | **Partial** | [NodeLifecycleEvents.cs](../../src/Atlas.Contracts/Graph/V1/NodeLifecycleEvents.cs)<br>[Program.cs](../../src/Atlas.Console/Program.cs) | — |
| [NFR-002](REQUIREMENTS.md#nfr-002) | Integration events survive process failure | Should | **Deferred** | — | — |

## Review notes and known gaps

<a id="rel-003"></a>
### REL-003 — Cycle prevention is host-local

The Console checks whether a proposed parent attachment would create a cycle, but the Graph boundary cannot currently guarantee this invariant by itself. A future Graph application service or graph-aware repository operation should enforce the rule for every host.

<a id="typ-004"></a>
### TYP-004 — Default Comment is host-local

The Console creation workflow supplies Comment by default. If this is a universal product rule, it should move behind a Graph application API or another shared policy so a future web/API host cannot omit it accidentally.

<a id="aut-002"></a>
### AUT-002 — Authorship exists before node authorization

Nodes retain an AuthorId, but most Console node mutations currently call Graph behavior without a boundary-owned authorization use case. The participant profile flow is the working example for the intended pattern.

<a id="evt-002"></a>
### EVT-002 and NFR-002 — Demonstration versus durable messaging

The synchronous in-memory publisher demonstrates subscription and broadcast clearly. It does not survive a crash, cross a process boundary, retry delivery, or provide idempotency. Those are deliberately deferred until Atlas adopts durable transport.

<a id="vot-001"></a>
### VOT-001 through VOT-011 — Voting Slice 1 is partially implemented

The current slice now includes the Voting model, Node rating policy, application service, participant-target duplicate check, in-memory and JSON repositories, Console casting flow, Node vote summaries, current-participant lookup, and a public voter list with participant-profile navigation.

Automated evidence currently proves valid vote creation, 0–10 rating validation, value immutability, sequential duplicate rejection, independent votes from different participants, empty summaries, target-filtered count and arithmetic mean, and current-participant vote lookup. JSON query/reconstitution behavior and timestamp preservation still need expanded automated tests.

The wider approved Voting baseline is intentionally larger than PTT-81. Vote change and undo, NodeTag voting, authenticated-account enforcement, boundary-level target eligibility, concurrent uniqueness, durable aggregation, and real-time service consistency remain future work. These gaps keep the affected requirements Partial or Approved rather than Verified.
## Suggested review order

1. Start with the five Graph fundamentals: GRA-001 through GRA-005.
2. Trace type and relationship rules through TYP-001 through REL-004.
3. Verify the Content reference and separate persistence through CON-001 and CON-002.
4. Walk through participant profile ownership using PAR-001, PAR-002, and AUT-001.
5. Review the known architectural gaps: REL-003, AUT-002, EVT-002, the VOT requirement area, and NFR-002.

During manual review, change a status only when all acceptance criteria support the new value. Add a missing test link when evidence exists; do not treat a Console demonstration as automated verification.
