# Local public think tank demo

The opt-in demo seed uses the five existing demo accounts. It adds 61 editorial nodes spanning environmental restoration, housing, civic trust, education, neighborhood design, public health and smaller practical questions. Some branches have multiple levels of children; no new multiple-parent examples are introduced. Importance ratings, reaction votes and threaded comments intentionally have uneven participation. Existing breadcrumb and moderation fixtures remain separate.

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

## Child-feed pagination walkthrough

Open **Can we save the Southern Resident orcas?** (node b1000000-0000-4000-8000-000000000201), then select **15 Solutions**. Fifteen proposed interventions (IDs ending 401–415) are direct children of this parent, all of type Solution. They have descriptions and full Markdown content, but do not imply actual completed projects or measured results.

Discovery's page size is three. Expect five responses with three items each; the UI appends cards, so the visible totals progress 3, 6, 9, 12, 15. Scroll to the tail to request more, or use Load more nodes when IntersectionObserver is unavailable. After the fifth page, the feed should say you are caught up and stop requesting pages.

In browser Network tools, each child-feed nodes request must keep ParentNodeId=b1000000-0000-4000-8000-000000000201 and NodeTypeId=60d8ba45-8b78-4593-838b-ffd15af69746, with Page increasing from 1 to 5. Confirm response pageSize=3, totalCount=15, and hasNextPage=false on page 5. Other public/session requests for individual cards are expected.

Switch to another contribution type and back: each tab starts its own scoped feed, without mixing children or carrying forward another tab's page number. A tall viewport can automatically fetch extra pages to fill available space; the network response size remains three.

The browser regression test uses intercepted API responses to check parent/type scoping and paging independently of SQL. This seed walkthrough is the separate real-database smoke check; it still needs to be run locally after refreshing disposable seed data.
