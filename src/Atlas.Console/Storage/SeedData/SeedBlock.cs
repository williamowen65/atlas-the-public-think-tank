using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    private static void SeedBlock(AtlasDataContext database)
    {
        database.BlockRows.AddRange(new BlockRow[]
        {
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000001"),
                Kind = "markdown",
                Markdown = "Useful ideas can come from anyone, and a small change in one place may inspire progress somewhere else. People of every age and background notice different needs and possibilities. Even an idea too big for one person to carry out can grow when others can see it, develop it, and connect it with ideas from other areas of life.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000002"),
                Kind = "markdown",
                Markdown = "Homelessness affects people's safety, health, and ability to build a stable life. Listening to people affected and understanding the causes can help communities address the issue in ways that meet people's needs.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000003"),
                Kind = "markdown",
                Markdown = "Access to essential services shapes what people are able to do in their daily lives. This goal asks us to notice where access breaks down and consider whether services truly work for the people who need them.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000004"),
                Kind = "markdown",
                Markdown = "People are already finding effective ways to address difficult problems. Studying what worked, for whom, and under what conditions could help others adapt promising approaches without assuming that one solution fits everywhere.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000005"),
                Kind = "markdown",
                Markdown = "Some decisions move forward before the most useful questions have been asked. Identifying what we don’t yet understand can direct attention toward evidence that would make future ideas and decisions stronger.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000006"),
                Kind = "markdown",
                Markdown = "The future is shaped partly by what people can imagine and work toward together. Sharing visions can make underlying values visible and help people decide which goals matter enough to pursue.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T15:20:00+00:00", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:03:58.010Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000007"),
                Kind = "markdown",
                Markdown = "Ranked voting would let people express more than one preference among candidates. Could using it in major U.S. elections give voters more meaningful choices and make room for more people to run? This proposal is worth exploring alongside its possible benefits, limits, and effects.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T16:47:12.781Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:47:12.781Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000008"),
                Kind = "markdown",
                Markdown = "If voters could express more than one preference, would more candidates consider running for office? Understanding candidates' decisions and examining elections that use ranked voting could help us evaluate this possibility.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T16:47:12.781Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T16:47:12.781Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000009"),
                Kind = "markdown",
                Markdown = "Southern Resident orcas are part of the life of the Pacific Northwest, yet their endangered population faces serious obstacles to recovery. This goal brings together questions, evidence, and proposals that could help them survive and thrive.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000010"),
                Kind = "markdown",
                Markdown = "Which pressures most limit the recovery of Southern Resident orcas, and how do they interact? Exploring prey availability, vessel disturbance, contaminants, and other factors can help people understand where action may make a difference.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000011"),
                Kind = "markdown",
                Markdown = "How did the Southern Resident population reach its present condition, and when did major changes occur? A shared timeline of population estimates and events can help distinguish longer patterns from short-term changes. Starting point: https://www.fisheries.noaa.gov/west-coast/endangered-species-conservation/saving-southern-resident-killer-whales",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000012"),
                Kind = "markdown",
                Markdown = "Southern Resident orcas depend on salmon, and restoring habitat used by their prey is one possible way to support recovery. Which places and restoration approaches could make a meaningful difference, and how would we measure their results?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:12:52.513Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000013"),
                Kind = "markdown",
                Markdown = "Internet access can connect people to information, services, education, and one another. What would it take to make reliable access available everywhere without a charge to the person using it? This idea invites exploration of coverage, funding, accessibility, and who would be responsible for keeping it working.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:19:30.587Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:19:30.587Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000014"),
                Kind = "markdown",
                Markdown = "Providing internet access without charging users would still require people and resources to build, operate, repair, and improve the service. What funding approaches could sustain reliable access over time, especially where it costs more to provide?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000015"),
                Kind = "markdown",
                Markdown = "One possibility is to fund a basic level of internet access as a public service. That might give more people a usable connection, but its scope, quality, cost, and governance would need to be worked out and compared with other approaches.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000016"),
                Kind = "markdown",
                Markdown = "A publicly funded service might offer very different levels of quality across communities if funding, infrastructure, or accountability varies. How could a basic service avoid leaving people with access in name but not in practice?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000017"),
                Kind = "markdown",
                Markdown = "If a service becomes widely relied upon, decisions about coverage, quality, privacy, and interruptions would matter to many people. Who should make those decisions, and how could users challenge them or hold the service accountable?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:37:05.793Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000018"),
                Kind = "markdown",
                Markdown = "Communities could build or operate networks to connect their residents, with support suited to local needs. Could this approach help provide reliable access where other models fall short, and what resources and responsibilities would it require? It is one possible answer to the funding question alongside a publicly funded basic service.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000019"),
                Kind = "markdown",
                Markdown = "Atlas is a place to explore questions, ideas, issues, and goals together. How should it help people understand one another, organize what they learn, and work on problems that matter? Ideas about Atlas itself can be proposed and examined here.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000020"),
                Kind = "markdown",
                Markdown = "Comments can help people ask for clarification or respond quickly, while nodes can give substantial questions and proposals a place in the graph. Where should the boundary be, and how should useful contributions move between the two?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000021"),
                Kind = "markdown",
                Markdown = "A comment may grow into a question or proposal that others would benefit from finding and developing. Could a person turn it into a typed node while preserving its author, context, and a link to the conversation that inspired it? This is a proposed feature to explore.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000022"),
                Kind = "markdown",
                Markdown = "People can disagree about causes, priorities, and possible solutions while still learning from one another. This goal invites ways to make those differences clear, examine the reasons behind them, and leave room for ideas to change.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T17:49:02.604Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000023"),
                Kind = "markdown",
                Markdown = "Atlas, the Public Think Tank is an idea for a place where anyone can contribute to understanding problems and developing possibilities. People bring questions, ideas, issues, goals, and evidence into a connected discussion so others can find them, evaluate their importance, and build on them. A thought that one person cannot pursue alone may become more useful when people with different experiences can work on it together.",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T18:02:56.236Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T18:02:56.236Z", System.Globalization.CultureInfo.InvariantCulture),
            },
            new BlockRow
            {
                Id = Guid.Parse("b3000000-0000-4000-8000-000000000024"),
                Kind = "markdown",
                Markdown = "Atlas organizes contributions as nodes. Each node has a type, title, and description; it can have parent nodes that show how it connects to larger questions or goals. A node may have more than one parent when it belongs in more than one branch. People can request useful kinds of child nodes, rate a node's importance, react to it, and find it through communities. Comments offer a lighter way to ask or respond. Which parts of this model make it easier to participate, and what still needs explaining?",
                CreatedAt = DateTimeOffset.Parse("2026-09-28T18:02:56.236Z", System.Globalization.CultureInfo.InvariantCulture),
                UpdatedAt = DateTimeOffset.Parse("2026-09-28T18:02:56.236Z", System.Globalization.CultureInfo.InvariantCulture),
            },
        });
    }
}
