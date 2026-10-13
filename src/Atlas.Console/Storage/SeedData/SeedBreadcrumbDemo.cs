using Atlas.Persistence;

namespace Atlas.ConsoleApp.Storage;

internal static partial class DemoData
{
    /// <summary>Ten ancestor links plus two intersecting routes for manual breadcrumb paging review.</summary>
    private static void SeedBreadcrumbDemo(AtlasDataContext database)
    {
        var timestamp = new DateTimeOffset(2026, 9, 28, 16, 0, 0, TimeSpan.Zero);
        var ideaType = Guid.Parse("0328981f-7b76-4bcb-ad04-99b4ed74fd23");
        var atlasRoot = Guid.Parse("b1000000-0000-4000-8000-000000000023");

        // Levels 1–10 follow one primary route back to the existing Atlas root.
        // Ordered parent lists make the chain the default even where routes intersect.
        for (var level = 1; level <= 12; level++)
        {
            var nodeId = BreadcrumbDemoId("b1000000", level);
            var documentId = BreadcrumbDemoId("b2000000", level);
            var blockId = BreadcrumbDemoId("b3000000", level);
            var parents = level switch
            {
                1 => new[] { atlasRoot },
                4 => new[] { BreadcrumbDemoId("b1000000", 3), BreadcrumbDemoId("b1000000", 11) },
                7 => new[] { BreadcrumbDemoId("b1000000", 6), BreadcrumbDemoId("b1000000", 12) },
                11 => new[] { BreadcrumbDemoId("b1000000", 2) },
                12 => new[] { BreadcrumbDemoId("b1000000", 4), BreadcrumbDemoId("b1000000", 11) },
                _ => new[] { BreadcrumbDemoId("b1000000", level - 1) }
            };
            var title = level switch
            {
                10 => "Breadcrumb demo: ten-level route",
                11 => "Breadcrumb demo: community route",
                12 => "Breadcrumb demo: research route",
                _ => $"Breadcrumb demo: level {level}"
            };

            database.NodeRows.Add(new NodeRow
            {
                Id = nodeId,
                Title = title,
                DescriptionId = documentId,
                TypeId = ideaType,
                AuthorId = DemoParticipants.User01Id,
                RequestedSubNodeTypeIds = [ideaType],
                ParentNodeIds = parents.ToList(),
                Status = "Active",
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
            database.DocumentRows.Add(new DocumentRow
            {
                Id = documentId,
                BlockIds = [blockId],
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
            database.BlockRows.Add(new BlockRow
            {
                Id = blockId,
                Kind = "markdown",
                Markdown = "Breadcrumb paging demonstration. Start at 'Breadcrumb demo: ten-level route' "
                    + "and use Explore parents, then N, to move toward the Atlas root three ancestors at a time. "
                    + "Open level 7 or level 4 and explore its parents to choose an alternative route.",
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            });
        }
    }

    // Reserve suffixes 101–112 independently in the node, document and block seed ranges.
    private static Guid BreadcrumbDemoId(string prefix, int level) =>
        Guid.Parse($"{prefix}-0000-4000-8000-{100 + level:D12}");
}
