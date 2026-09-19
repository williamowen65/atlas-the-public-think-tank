using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;

namespace Atlas.ConsoleApp;

/// <summary>Provides the console test harness for ordered description blocks.</summary>
public static class DescriptionBlockCommands
{
    public static void Run(
        Node node,
        IDocumentRepository documents,
        Guid actorParticipantId)
    {
        node.EnsureAuthoredBy(actorParticipantId);

        var document = documents.GetById(
            new DocumentId(node.DescriptionId.Value))
            ?? throw new InvalidOperationException(
                "The node's description document could not be found.");

        var managing = true;

        while (managing)
        {
            Console.Clear();
            Console.WriteLine("MANAGE DESCRIPTION BLOCKS");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"Node:     {node.Title}");
            Console.WriteLine($"Document: {document.Id}");
            Console.WriteLine();

            WriteBlocks(document, documents);

            Console.WriteLine();
            Console.WriteLine("1. Add block");
            Console.WriteLine("2. Update block");
            Console.WriteLine("3. Reorder block");
            Console.WriteLine("4. Remove block");
            Console.WriteLine("5. Return to node");
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddBlock(document, documents);
                    break;
                case "2":
                    UpdateBlock(document, documents);
                    break;
                case "3":
                    ReorderBlock(document, documents);
                    break;
                case "4":
                    RemoveBlock(document, documents);
                    break;
                case "5":
                    managing = false;
                    break;
                default:
                    ConsoleUi.Pause("Please select an option from 1 through 5.");
                    break;
            }
        }
    }

    private static void WriteBlocks(
        Document document,
        IDocumentRepository documents)
    {
        var blocks = documents.GetBlocks(document).ToList();

        if (blocks.Count == 0)
        {
            Console.WriteLine("(No blocks)");
            return;
        }

        for (var index = 0; index < blocks.Count; index++)
        {
            var block = blocks[index];
            Console.WriteLine(
                $"{index + 1}. {block.Kind,-16} {block.Id}  {Summary(block)}");
        }
    }

    private static string Summary(ContentBlock block) => block switch
    {
        MarkdownTextBlock text => Compact(text.Markdown),
        ImageBlock image => $"{Compact(image.AltText)} ({image.Url})",
        VideoBlock video => $"{Compact(video.Caption)} ({video.Url})",
        LinkPreviewBlock link => $"{Compact(link.Title)} ({link.Url})",
        PollReferenceBlock poll => poll.PollId.ToString(),
        ChartReferenceBlock chart => $"{Compact(chart.Title)} ({chart.ChartId})",
        _ => string.Empty
    };

    private static string Compact(string? value)
    {
        var oneLine = (value ?? string.Empty)
            .ReplaceLineEndings(" ")
            .Trim();

        return oneLine.Length <= 45
            ? oneLine
            : $"{oneLine[..42]}...";
    }

    private static void AddBlock(
        Document document,
        IDocumentRepository documents)
    {
        Console.WriteLine();
        Console.WriteLine("1. Markdown text");
        Console.WriteLine("2. Image");
        Console.WriteLine("3. Video");
        Console.WriteLine("4. Link preview");
        Console.WriteLine("5. Poll reference");
        Console.WriteLine("6. Chart reference");
        Console.WriteLine("0. Cancel");
        Console.Write("Block type: ");

        var now = DateTimeOffset.UtcNow;
        ContentBlock? block = Console.ReadLine() switch
        {
            "1" => NewMarkdown(now),
            "2" => NewImage(now),
            "3" => NewVideo(now),
            "4" => NewLinkPreview(now),
            "5" => NewPollReference(now),
            "6" => NewChartReference(now),
            _ => null
        };

        if (block is null)
        {
            return;
        }

        Console.Write($"Position (1-{document.BlockIds.Count + 1}, blank for end): ");
        var positionText = Console.ReadLine();
        int? index = null;

        if (!string.IsNullOrWhiteSpace(positionText))
        {
            if (!int.TryParse(positionText, out var position) ||
                position < 1 || position > document.BlockIds.Count + 1)
            {
                ConsoleUi.Pause("That position is not valid.");
                return;
            }

            index = position - 1;
        }

        documents.SaveBlock(block);
        document.AddBlock(block.Id, DateTimeOffset.UtcNow, index);
        documents.Save(document);
        ConsoleUi.Pause($"Added {block.Kind} block {block.Id}.");
    }

    private static void UpdateBlock(
        Document document,
        IDocumentRepository documents)
    {
        var block = SelectBlock(document, documents, "Block to update");
        if (block is null) return;

        var now = DateTimeOffset.UtcNow;

        switch (block)
        {
            case MarkdownTextBlock text:
                Console.Write("New Markdown: ");
                text.Update(Console.ReadLine() ?? string.Empty, now);
                break;
            case ImageBlock image:
                Console.Write($"Image URL ({image.Url}): ");
                var imageResource = Keep(Console.ReadLine(), image.Url);
                Console.Write($"Alt text ({image.AltText}): ");
                var alt = Keep(Console.ReadLine(), image.AltText);
                Console.Write($"Caption ({image.Caption}): ");
                image.Update(imageResource, alt, Console.ReadLine(), now);
                break;
            case VideoBlock video:
                Console.Write($"Video URL ({video.Url}): ");
                var videoResource = Keep(Console.ReadLine(), video.Url);
                Console.Write($"Caption ({video.Caption}): ");
                video.Update(videoResource, Console.ReadLine(), now);
                break;
            case LinkPreviewBlock link:
                Console.Write($"URL ({link.Url}): ");
                var url = Keep(Console.ReadLine(), link.Url.ToString());
                Console.Write($"Title ({link.Title}): ");
                var title = Keep(Console.ReadLine(), link.Title);
                Console.Write($"Description ({link.Description}): ");
                link.UpdatePreview(url, title, Console.ReadLine(), now);
                break;
            case PollReferenceBlock:
                ConsoleUi.Pause("This poll reference has no editable block content yet.");
                return;
            case ChartReferenceBlock chart:
                Console.Write($"Title ({chart.Title}): ");
                chart.Update(chart.ChartId, Keep(Console.ReadLine(), chart.Title), now);
                break;
        }

        documents.SaveBlock(block);
        ConsoleUi.Pause($"Updated {block.Kind} block; its ID remains {block.Id}.");
    }

    private static void ReorderBlock(Document document, IDocumentRepository documents)
    {
        var block = SelectBlock(document, documents, "Block to move");
        if (block is null) return;

        Console.Write($"New position (1-{document.BlockIds.Count}): ");
        if (!int.TryParse(Console.ReadLine(), out var position) ||
            position < 1 || position > document.BlockIds.Count)
        {
            ConsoleUi.Pause("That position is not valid.");
            return;
        }

        document.MoveBlock(
            block.Id,
            position - 1,
            DateTimeOffset.UtcNow);
        documents.Save(document);
        ConsoleUi.Pause($"Moved block {block.Id} to position {position}.");
    }

    private static void RemoveBlock(Document document, IDocumentRepository documents)
    {
        var block = SelectBlock(document, documents, "Block to remove");
        if (block is null) return;

        document.RemoveBlock(block.Id, DateTimeOffset.UtcNow);
        documents.Save(document);
        ConsoleUi.Pause(
            $"Removed block {block.Id} from the description. Its stored record is retained.");
    }

    private static ContentBlock? SelectBlock(
        Document document,
        IDocumentRepository documents,
        string prompt)
    {
        var blocks = documents.GetBlocks(document).ToList();
        if (blocks.Count == 0)
        {
            ConsoleUi.Pause("This description has no blocks.");
            return null;
        }

        Console.Write($"{prompt} (1-{blocks.Count}, 0 cancels): ");
        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 || selection > blocks.Count)
        {
            ConsoleUi.Pause("That block selection is not valid.");
            return null;
        }

        return selection == 0 ? null : blocks[selection - 1];
    }

    private static MarkdownTextBlock NewMarkdown(DateTimeOffset now)
    {
        Console.Write("Markdown: ");
        return new MarkdownTextBlock(Console.ReadLine() ?? string.Empty, now);
    }

    private static ImageBlock NewImage(DateTimeOffset now)
    {
        Console.Write("Image URL: ");
        var resourceUrl = Console.ReadLine();
        Console.Write("Alternative text: ");
        var altText = Console.ReadLine();
        Console.Write("Caption (optional): ");
        return new ImageBlock(resourceUrl ?? string.Empty, altText ?? string.Empty, Console.ReadLine(), now);
    }

    private static VideoBlock NewVideo(DateTimeOffset now)
    {
        Console.Write("Video URL: ");
        var resourceUrl = Console.ReadLine();
        Console.Write("Caption (optional): ");
        return new VideoBlock(resourceUrl ?? string.Empty, Console.ReadLine(), now);
    }

    private static LinkPreviewBlock NewLinkPreview(DateTimeOffset now)
    {
        Console.Write("URL: ");
        var url = Console.ReadLine();
        Console.Write("Title: ");
        var title = Console.ReadLine();
        Console.Write("Description (optional): ");
        return new LinkPreviewBlock(url ?? string.Empty, title ?? string.Empty, Console.ReadLine(), now);
    }

    private static PollReferenceBlock NewPollReference(DateTimeOffset now)
    {
        return new PollReferenceBlock(now);
    }

    private static ChartReferenceBlock NewChartReference(DateTimeOffset now)
    {
        Console.Write("Title (optional): ");
        return new ChartReferenceBlock(Console.ReadLine(), now);
    }

    private static string Keep(string? replacement, string current) =>
        string.IsNullOrWhiteSpace(replacement) ? current : replacement;

}
