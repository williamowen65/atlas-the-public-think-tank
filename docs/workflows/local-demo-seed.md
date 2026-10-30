# Local public think tank demo

The opt-in demo seed uses the five existing demo accounts. It adds 46 editorial nodes spanning environmental restoration, housing, civic trust, education, neighborhood design, public health and smaller practical questions. Some branches have multiple levels of children; no new multiple-parent examples are introduced. Importance ratings, reaction votes and threaded comments intentionally have uneven participation. Existing breadcrumb and moderation fixtures remain separate.

## Editorial variety for UI review

The 46 editorial nodes previously all requested Question, Idea and Issue. They now request topic-specific combinations of the existing catalog types, including Solution, Evidence, Location, Goal and Vision. For example, salmon restoration invites locations and goals, classroom AI invites visions and evidence, and broadband planning invites locations, solutions and questions. Existing child types remain included in their parents' requested lists.

Practical intervention nodes use Solution where appropriate; the safe-school-route priority uses Goal. This lets the feed show populated contribution categories as well as zero-count requests. No new catalog types or migrations are needed.

Short descriptions now vary in length, with fuller introductions up to the backend's 280-character maximum. Full Markdown descriptions remain in their existing blocks. Counts and relationships are still based on seeded data, not fabricated by the UI.

These edits only affect a newly seeded empty database. The normal seed command does not overwrite an existing database or user content. Use the disposable-database procedure below only after preserving anything you need; pulling the branch alone will not change existing rows.

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
