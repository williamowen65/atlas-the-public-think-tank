# Participants Authorization Review

PTT-120 review of authorization at the `Atlas.Participants` boundary.

## Boundary summary

Participants owns Atlas's public participant profile and participant lifecycle state: participant ID, display name, biography, active state, and timestamps. Authentication credentials and account-security state do **not** belong to this domain.

ASP.NET Core Identity owns email/account credentials, password hashes, email confirmation, lockout, tokens, and global role membership. The Identity user ID and Participant ID are intentionally the same GUID, but the two boundaries have different responsibilities.

## Authorization inventory

| Boundary / operation | Data/action | Required access | Current enforcement | Test/evidence | Result / follow-up |
|---|---|---|---|---|---|
| Browse participant profile | Read display name, bio, active state, timestamps | Public | Participant repository/display/query paths expose profile-domain state | Existing participant browsing behavior and requirements traceability | **Covered** as public Atlas data |
| Browse participant contributions | Read public content associated with participant | Public | Host/query composition over public Atlas data | Existing Participant/Console browsing behavior | **Covered at current host**; future API read contract still needs endpoint tests |
| Update display name / bio | Write own public profile | Authenticated owner | `UpdateParticipantProfile.Execute(actorId, profileId,...)` requires actor and profile IDs to match before loading or saving | `UpdateParticipantProfileTests` proves owner success and foreign-actor rejection without state change/save | **Covered** |
| Update another participant's profile | Write foreign profile | Not permitted for Member | Same ownership check rejects mismatched actor/profile IDs | `Execute_WhenActorDoesNotOwnProfile_ThrowsWithoutSaving` | **Covered** |
| Read password hash, confirmation state, lockout, tokens, global roles | Private account/security data | Identity/account-management boundary only; never ordinary public profile access | Data is owned by ASP.NET Core Identity rather than `Participant` | Identity architecture/model and Identity tests | **Covered structurally**; future API must not expose these as Participant fields |
| Register account/profile | Create linked account + Participant | Public registration subject to Identity validation; caller cannot choose ID or privileged role | `AtlasAccounts.RegisterAsync` creates the Participant and Identity account atomically and assigns Member | Identity SQL/model tests | **Covered** |
| Resolve current Participant from authenticated account | Establish trusted Atlas actor | Authenticated eligible account | Identity maps the authenticated framework user-ID claim to the linked Participant and checks account/profile eligibility | Identity policy/session tests | **Covered at current host**; future API must use this mapping rather than request-supplied actor identity |
| Deactivate participant | Change participant lifecycle state | **Not yet classified as a user-facing authorization workflow** | `Participant.Deactivate` is public domain behavior and does not itself accept an actor | Domain lifecycle behavior | **Review/follow-up:** define who may deactivate an account/profile before exposing this operation through a host |

## Findings

### Public profile versus private account data

The boundary is intentionally split. `Participant` contains only Atlas profile/lifecycle information. Credentials and account-security information live in Identity. This prevents ordinary Participant profile queries from naturally returning password hashes, tokens, confirmation state, lockout information, or role-storage details.

Email currently belongs to the Identity account rather than the public Participant profile. If Atlas later chooses to display an email address or contact method publicly, that should be modeled as an explicit profile/privacy feature rather than exposing Identity account data.

### Self-edit authorization

`UpdateParticipantProfile` is the application boundary for profile editing. It receives both the trusted actor ID and target profile ID and rejects them when they differ. The entity's lower-level profile mutation methods are internal, preventing normal external hosts from bypassing that workflow.

The future REST API must derive the actor ID from authenticated Identity. A request may identify the target profile, but a request-supplied actor ID must never be accepted as proof of identity.

### Participant deactivation needs an explicit policy

`Participant.Deactivate(DateTimeOffset)` currently expresses lifecycle behavior but not caller authorization. That is not automatically a vulnerability because the presence of a public domain method does not itself expose a remote operation. However, before deactivation is exposed through an application/API workflow, Atlas should decide whether it is self-service account deactivation, Administrator-only action, moderation/sanction behavior, or some combination with distinct workflows.

This is an authorization-policy decision rather than something PTT-120 should guess.

## PTT-120 result

Participant profile editing has an explicit and tested owner boundary. Public profile data is cleanly separated from Identity-owned private account/security data.

The only authorization question identified in this pass is **participant deactivation**. No speculative API code is required now; its intended actor/policy should be decided before a host exposes that capability.
