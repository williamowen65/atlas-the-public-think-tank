# Local public think tank demo

The opt-in demo seed uses the five existing demo accounts. It adds 46 editorial nodes spanning environmental restoration, housing, civic trust, education, neighborhood design, public health and smaller practical questions. Some branches have multiple levels of children; no new multiple-parent examples are introduced. Importance ratings, reaction votes and threaded comments intentionally have uneven participation. Existing breadcrumb and moderation fixtures remain separate.

The implementation is in `src/Atlas.Application/Storage/SeedData/SeedPublicThinkTank.cs` and `SeedPublicThinkTankCommunities.cs`. Both are called from `DemoData.Seed`.

## Rebuild a disposable local database

**Warning:** Dropping the database permanently destroys *all* its data, including any nodes, accounts or comments created locally. Back up anything you want to retain, and verify `ATLAS_SQL_CONNECTION_STRING` points to your disposable development database. Stop the API and console first.

From the repository root, with the .NET SDK and `dotnet-ef` installed:

```powershell
dotnet ef database drop --project src/Atlas.Persistence --force
```

The design-time factory reads `ATLAS_SQL_CONNECTION_STRING` from the environment or shared user secrets. The console automatically runs EF migrations on startup, then seeds only an empty database:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --seed-demo
```

Restart the API and Storybook afterward. Demo sign-in accounts are `demo01@example.test` through `demo05@example.test`; the demo password is printed by the seed command. **Never use these credentials or this dataset in production.**

If `dotnet ef` cannot load the design-time factory, first verify the .NET EF tooling and your SQL connection configuration. Do not run the drop command against an unknown connection string.
