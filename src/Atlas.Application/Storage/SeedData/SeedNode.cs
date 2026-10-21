using Atlas.Persistence;

namespace Atlas.Application.Storage;

internal static partial class DemoData
{
    private static void SeedNode(AtlasDataContext database)
    {
        database.NodeRows.AddRange(new NodeRow[]
        {
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000001"),
                Title = "What ideas could help make the world better?",
                ShortDescription = "Share ideas that could improve life, then connect and develop them with people who see different needs and possibilities.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000001"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"), Guid.Parse("a3000000-0000-4000-8000-000000000001") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000002"),
                Title = "Homelessness",
                ShortDescription = "Explore the causes of homelessness and responses shaped by the needs and experiences of people affected.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000002"),
                TypeId = Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"),
                AuthorId = DemoParticipants.User02Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("60d8ba45-8b78-4593-838b-ffd15af69746") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000003"),
                Title = "Make essential services accessible to everyone",
                ShortDescription = "Identify barriers to essential services and develop ways to make them work for everyone who needs them.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000003"),
                TypeId = Guid.Parse("a3000000-0000-4000-8000-000000000001"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000004"),
                Title = "Learn from solutions that already work",
                ShortDescription = "Find approaches that have succeeded elsewhere and consider how they could be adapted to new places and circumstances.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000004"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User04Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f"), Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("60d8ba45-8b78-4593-838b-ffd15af69746") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000005"),
                Title = "What do we need to understand better?",
                ShortDescription = "Collect questions where better evidence, context, or understanding could help people make progress.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000005"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User05Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000006"),
                Title = "A future worth building",
                ShortDescription = "Describe the future we want and connect practical steps that could help us build it.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000006"),
                TypeId = Guid.Parse("a3000000-0000-4000-8000-000000000002"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a3000000-0000-4000-8000-000000000001"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:20:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T15:43:00+00:00",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000007"),
                Title = "Use ranked voting in major U.S. elections",
                ShortDescription = "Explore ranked voting as a way to give voters more meaningful choices in major U.S. elections.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000007"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [  ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000001") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T16:46:57.176Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T16:46:57.176Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000008"),
                Title = "Would ranked voting encourage more people to run for office?",
                ShortDescription = "Consider whether ranked voting would reduce barriers for candidates and broaden the choices available to voters.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000008"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User02Id,
                RequestedSubNodeTypeIds = [  ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000007") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T16:46:57.176Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T16:46:57.176Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000009"),
                Title = "Help Southern Resident orcas recover",
                ShortDescription = "Connect evidence, questions, and actions that could support Southern Resident orca recovery.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000009"),
                TypeId = Guid.Parse("a3000000-0000-4000-8000-000000000001"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000001") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000010"),
                Title = "What most limits Southern Resident orca recovery?",
                ShortDescription = "Compare the pressures on Southern Resident orcas to understand which changes could most improve their chances of recovery.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000010"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000009"), Guid.Parse("b1000000-0000-4000-8000-000000000005") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000011"),
                Title = "How has the Southern Resident orca population changed over time?",
                ShortDescription = "Examine population trends and what they reveal about Southern Resident orca recovery.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000011"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000010") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000012"),
                Title = "Restore salmon habitat that supports orca prey",
                ShortDescription = "Improve salmon spawning and rearing habitat to strengthen a vital food source for Southern Resident orcas.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000012"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000009") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:12:32.348Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000013"),
                Title = "Make internet access free and available everywhere",
                ShortDescription = "Explore ways to make reliable internet access available to everyone without a direct charge for basic service.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000013"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User04Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000003"), Guid.Parse("b1000000-0000-4000-8000-000000000006") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:19:14.273Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:19:14.273Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000014"),
                Title = "How could free internet access be funded and maintained?",
                ShortDescription = "Compare funding and maintenance approaches that could keep universal internet access reliable over time.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000014"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User02Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"), Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000013") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000015"),
                Title = "Publicly fund a basic level of internet access",
                ShortDescription = "Consider publicly funding basic internet service so cost does not prevent people from getting online.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000015"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"), Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000014") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000016"),
                Title = "Public funding could leave some communities with inadequate service",
                ShortDescription = "Investigate how public funding could deliver equitable service across communities with different needs and infrastructure.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000016"),
                TypeId = Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"),
                AuthorId = DemoParticipants.User04Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000015") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000017"),
                Title = "Who would oversee the service and protect users' interests?",
                ShortDescription = "Explore oversight that would protect privacy, accountability, and users’ interests in publicly supported internet service.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000017"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User05Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000014") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:36:49.048Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000018"),
                Title = "Support community-owned internet networks",
                ShortDescription = "Help communities own and operate internet infrastructure that responds to local needs.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000018"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User05Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000014") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000019"),
                Title = "How should Atlas help people think and work together?",
                ShortDescription = "Explore how Atlas can help people connect ideas, examine evidence, and develop useful responses together.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000019"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("a3000000-0000-4000-8000-000000000001"), Guid.Parse("53057519-9a26-4b02-a884-56dcf82159ed") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000023") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T18:02:36.726Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000020"),
                Title = "When should a contribution be a node instead of a comment?",
                ShortDescription = "Clarify when a contribution deserves its own place in the graph and when it works best as a comment.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000020"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User02Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000019") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000021"),
                Title = "Let people turn a comment into a node",
                ShortDescription = "Allow a useful comment to become a node while preserving the discussion and context that inspired it.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000021"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User03Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000020") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000022"),
                Title = "Make disagreement useful",
                ShortDescription = "Find ways to turn different perspectives into clearer questions, stronger evidence, and better ideas.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000022"),
                TypeId = Guid.Parse("a3000000-0000-4000-8000-000000000001"),
                AuthorId = DemoParticipants.User04Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("df57b39d-efba-4398-84e0-6a7d4214287f") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000019") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T17:48:43.289Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000023"),
                Title = "Atlas, the Public Think Tank",
                ShortDescription = "An open public think tank where people connect problems, questions, evidence, and ideas to build shared understanding.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000023"),
                TypeId = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23"), Guid.Parse("a3000000-0000-4000-8000-000000000001") ],
                ParentNodeIds = [  ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T18:02:36.726Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T18:02:36.726Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
            new NodeRow
            {
                Id = Guid.Parse("b1000000-0000-4000-8000-000000000024"),
                Title = "How does Atlas work?",
                ShortDescription = "Learn how nodes, connections, content, and importance ratings help people explore and develop ideas in Atlas.",
                DescriptionId = Guid.Parse("b2000000-0000-4000-8000-000000000024"),
                TypeId = Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"),
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ Guid.Parse("a2e82ee2-12ba-49fc-9160-c2b864d4f42f"), Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23") ],
                ParentNodeIds = [ Guid.Parse("b1000000-0000-4000-8000-000000000023") ],
                Status = "Active",
                CreatedAt = DateTimeOffset.Parse(
                    "2026-09-28T18:02:36.726Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
                UpdatedAt = DateTimeOffset.Parse(
                    "2026-09-28T18:02:36.726Z",
                    System.Globalization.CultureInfo.InvariantCulture
                ),
            },
        });
    }
}
