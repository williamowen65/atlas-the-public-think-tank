# Atlas.Identity

Infrastructure for ASP.NET Core Identity accounts linked by GUID to independent Atlas Participants. Use `AddAtlasIdentity(connectionString)` in an authenticated host and `AtlasAccounts.RegisterAsync` for atomic new account/profile creation. Framework managers own passwords, tokens, lockout, and sign-in.

See [architecture and host integration](../../docs/architecture/IDENTITY.md) and [developer-generated migration setup](../../docs/PTT-94-Identity.md). This library does not expose HTTP endpoints. Atlas.Console now uses its account coordinator, sign-in manager, and policies for authenticated console sessions.
