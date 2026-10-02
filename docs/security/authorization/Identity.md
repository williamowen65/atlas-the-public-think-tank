# Identity Authorization

| Operation | Access | How is the actor established? | What enforces permission? | Evidence | Result |
|---|---|---|---|---|---|
| Register | Anonymous | No existing actor; Identity creates a new account | `AtlasAccounts.RegisterAsync` creates its own shared GUID, validates Identity password/account rules, creates Participant atomically, assigns only Member | Identity SQL tests | Covered |
| Sign in | Anonymous presenting credentials | ASP.NET Core Identity verifies email/password and account state | Identity/SignInManager; Console session then resolves active Participant | Identity tests + Console session | Covered at current host |
| Resolve authenticated Participant | Authenticated principal | Framework principal is produced by authentication | `ResolveParticipantAsync` requires authenticated principal, persisted user, confirmed email, not locked out, active Participant | Identity SQL tests | Covered |
| Authorize Member policy | Authenticated principal | Same | `AtlasRoleHandler` resolves active Participant then checks persisted role membership | Identity tests | Covered |
| Authorize GlobalModerator policy | Authenticated principal | Same | Policy accepts persisted GlobalModerator/Administrator role; does not rely only on claims | Identity tests | Covered |
| Authorize Administrator policy | Authenticated principal | Same | Persisted Administrator role required | Identity tests | Covered |
| Supply moderator fact to Moderation | Authenticated actor ID from host | Host supplies actor ID established by authentication | `IdentityModeratorAuthorization` checks user + active Participant + confirmed/unlocked account + persisted GlobalModerator/Admin role | Identity/Moderation tests | Covered |
| Confirm email | User possessing valid confirmation token | Token is generated/validated by ASP.NET Core Identity; current Console command is development-only substitute for email delivery | `UserManager.ConfirmEmailAsync` | Identity tests | Identity covered; delivery/HTTP endpoint future |
| Password change/reset | Authenticated user or valid reset-token flow | Identity authentication/token mechanism | ASP.NET Core Identity password/token validation | Identity SQL tests | Covered at service level |
| Two-factor authentication | Future authenticated login flow | Not implemented in Console | Deferred to PTT-127 API/web authentication flow | PTT-127 | Deferred |

## Review note

Identity answers **who is this actor?** and owns global account/role state. It does not replace domain authorization such as node authorship, profile ownership, community ownership, or contextual community roles.
