# Local public think tank demo

The **opt-in seed** creates a representative Atlas dataset for development and UI review. It is intended for an **empty, disposable database**, not production.

## At a glance

| Fixture | What it exercises |
|---|---|
| Five demo accounts | Sign-in and participant data |
| 61 editorial nodes | Environmental, housing, civic, education, public-health, and neighborhood topics |
| Varied contribution types | Populated and zero-count child categories |
| Uneven votes, reactions, comments | Realistic feed participation |
| Moderation cases | Pending, hidden, and review-requested states |
| Orca Solutions child feed | Five pages of three nodes |

Some topics have multiple child levels; this seed adds no new multiple-parent examples. Existing breadcrumb and moderation fixtures remain separate.

## Editorial content

Previously, 46 editorial nodes all requested Question, Idea, and Issue. They now use more suitable existing types such as **Solution, Evidence, Location, Goal, and Vision**.

For example:

- **Salmon restoration:** locations and goals
- **Classroom AI:** visions and evidence
- **Broadband planning:** locations, solutions, and questions
- **Practical interventions:** Solution where appropriate
- **Safe-school-route priority:** Goal

Parents continue to request the types their existing children use. Descriptions vary in length, up to the backend's **280-character** short-description limit. Full Markdown descriptions remain in their content blocks. Counts and relationships are calculated from seeded records rather than fabricated in the UI.

The implementation lives in `src/Atlas.Application/Storage/SeedData/SeedPublicThinkTank.cs` and `SeedPublicThinkTankCommunities.cs`, called from `DemoData.Seed`.

**Existing data is untouched:** Changes appear only when an empty database is seeded. Pulling this branch does not overwrite local records or require a new catalog migration.

## Reset and seed a disposable database

> **Destructive operation.** Dropping the configured database permanently removes *all* its accounts, nodes, comments, and other content. Back up anything you need. Check `ATLAS_SQL_CONNECTION_STRING` points to a disposable development database; never use an unknown or production connection. Stop API and Console before starting.

From the repository root, with the .NET SDK and `dotnet-ef` installed:

### 1. Drop the disposable database

```powershell
dotnet ef database drop --project src/Atlas.Persistence --force
```

### 2. Apply migrations and set up search

The design-time factory reads `ATLAS_SQL_CONNECTION_STRING` from environment variables or shared user secrets. The explicit setup applies EF migrations and creates SQL Server full-text indexes:

```powershell
dotnet run --project src/hosts/Atlas.Console -- --setup-database
```

### 3. Seed and reopen the UI

```powershell
dotnet run --project src/hosts/Atlas.Console -- --seed-demo
```

Restart the API and Storybook after seeding.

| Demo setting | Value |
|---|---|
| Accounts | `demo01@example.test` to `demo05@example.test` |
| Password | Printed by the seed command |
| Use | Local development only; never production |

If EF cannot load the design-time factory, check your installed EF tooling and SQL connection configuration **before** any drop command.

## Moderation fixtures

These synthetic cases let you review queue, hidden-content, and restoration states **without submitting real reports**.

The seed creates four `ModerationCaseRows`:

| Node | State | Intended check |
|---|---|---|
| **Homelessness** (Demo User 02) | Pending report | Appears in moderator queue |
| **Make essential services accessible to everyone** (Demo User 03) | Hidden, no review request | Author can edit and later request review |
| **Learn from solutions that already work** (Demo User 04) | Hidden, review requested | Ready for restoration review |
| **Would ranked voting encourage more people to run for office?** | Hidden grandchild, no review request | Shows true ancestor breadcrumb |

The ranked-voting grandchild is under **Use ranked voting in major U.S. elections**, itself under **What ideas could help make the world better?**

### Historical engagement

The three hidden nodes have **pre-decision** importance ratings and curated-reaction votes:

| Hidden node | Ratings | Average | Reaction scores |
|---|---|---|---|
| Make essential services accessible to everyone | 8, 10 | 9.0 (2 votes) | Promising +1; Practical +1 |
| Learn from solutions that already work | 5, 7, 9 | 7.0 (3 votes) | Innovative +1; Hopeful +1 |
| Would ranked voting encourage more people to run for office? | 6, 8 | 7.0 (2 votes) | Uncertain +1 |

### Review the fixtures

1. Sign in with a **global moderator** account and enable **Include hidden nodes**.
2. Inspect the three hidden cards in Discovery. Existing votes/reactions should display, but their controls stay disabled.
3. Verify the hidden grandchild displays its two actual ancestors.
4. Sign out or disable hidden inclusion. Confirm normal public redaction.
5. Try a new vote on a hidden Node; the backend must reject it.

**Important boundaries:**

- These fixtures reuse existing Node and Participant IDs; they are **not real submitted reports**.
- A seeded review-request status does not prove a demo author edited that Node.
- The seed does **not** give a demo account moderator permissions. Use [trusted moderator provisioning](../PTT-94-Identity.md#trusted-moderator-provisioning-ptt-159).
- These are seed-only historical engagement records. Author-facing web behavior and privacy still need separate testing.
- Never drop existing data without preserving it first.

## Child-feed pagination walkthrough

**Goal:** Confirm the Orca Solutions tab shows 15 nodes as **five pages of three**, without mixing other contribution types.

### 1. Open the feed

Navigate to **Can we save the Southern Resident orcas?** (Node `b1000000-0000-4000-8000-000000000201`) and select **15 Solutions**.

- The 15 direct children have IDs ending **401–415** and type **Solution**.
- They contain descriptions and full Markdown, but do not claim completed interventions or measured results.

### 2. Check paging

Discovery requests three nodes per page. Expected visible totals after each appended page:

`3 → 6 → 9 → 12 → 15`

The UI requests further pages near the scroll tail or through **Load more nodes** when `IntersectionObserver` is unavailable. After page five, it should indicate the feed is caught up and stop requesting pages.

### 3. Inspect browser Network requests

Every child-feed request must keep:

| Parameter | Expected value |
|---|---|
| `ParentNodeId` | `b1000000-0000-4000-8000-000000000201` |
| `NodeTypeId` | `60d8ba45-8b78-4593-838b-ffd15af69746` |
| `Page` | Increases from 1 through 5 |
| Response `pageSize` | 3 |
| Response `totalCount` | 15 |
| Response `hasNextPage` | `false` on page 5 |

Other card-specific public/session requests are expected.

### 4. Switch contribution tabs

Switch to another type and back. Each tab starts its own scoped feed; children and page numbers must not leak across tabs. A tall viewport may fetch additional pages automatically, but each response still contains at most three items.

**Test coverage:** The browser regression intercepts API responses to verify paging independently of SQL. This real-database seed walkthrough is a separate smoke check and still needs a local run after reseeding.
