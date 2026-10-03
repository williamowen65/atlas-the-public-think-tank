# Participants Authorization

Each row answers two security questions: **how is the actor established?** and **what enforces permission once the actor is known?**

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| [Browse participant profile](../../../src/Atlas.Participants/Participants/Participant.cs) | Public | No actor required | Public profile read; Participant contains profile/lifecycle data, not Identity credentials | Participant browsing + Identity boundary | Covered |
| [Browse participant contributions](../../../src/Atlas.Console/Participants/ParticipantCommands.cs) | Public | No actor required | Public Atlas read paths | Participant/Console browsing | Covered at current host |
| [Update display name / bio](../../../src/Atlas.Participants/Profiles/UpdateParticipantProfile.cs) | Owner | Host derives Participant ID from authenticated Identity | `UpdateParticipantProfile` requires actor ID = target profile ID before save | `UpdateParticipantProfileTests` | Covered |
| [Update another profile](../../../src/Atlas.Participants/Profiles/UpdateParticipantProfile.cs) | Not permitted to ordinary Member | Host derives actor from authenticated Identity | Same ownership comparison rejects foreign actor | Foreign-actor update test | Covered |
| [Register account/profile](../../../src/Atlas.Identity/AtlasAccounts.cs) | Registration | Identity registration establishes the new account; caller does not choose an existing actor | `AtlasAccounts.RegisterAsync` creates linked Identity user + Participant and assigns Member | Identity SQL tests | Covered |
| [Resolve “My Profile”](../../../src/Atlas.Identity/AtlasAccounts.cs) | Authenticated participant | Host/session maps authenticated Identity user ID to Participant ID | Host exposes “my” workflow only when that mapping exists | `ConsoleIdentitySession` / Identity tests | Covered at current host |
| [Read password hash, confirmation, lockout, tokens, global roles](../../../src/Atlas.Identity/AtlasAuthorization.cs) | Not public Participant data | Identity account context | Data is owned by ASP.NET Core Identity, not Participant | Identity model/architecture | Covered structurally; future API must not expose as profile data |
| [Deactivate participant](../../../src/Atlas.Participants/Participants/Participant.cs) | Undecided | No user-facing authenticated workflow established yet | `Participant.Deactivate` enforces lifecycle behavior, **not who may invoke it** | Participant domain | Policy needed before exposure |

## Review note

The disabled anonymous **My Profile** menu item is user-interface behavior, not the ownership security boundary. The important protection for profile mutation is the application workflow's actor/target comparison. The host's responsibility is to supply the actor from authenticated Identity rather than caller input.
