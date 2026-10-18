using Atlas.Application;
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
        Guid actorParticipantId,
        INodeRepository nodes
    )
    {
        // Description composition can only be changed by the node's author.
        node.EnsureAuthoredBy(actorParticipantId);

        // Graph owns only DescriptionId; Content owns and loads the document itself.
        var document = documents.GetById(
            new DocumentId(node.DescriptionId.Value))
            ?? throw new InvalidOperationException(
                "The node's description document could not be found.");

        var service = new NodeDescriptionService(nodes, documents);
        var managing = true;

        while (managing)
        {
            document = documents.GetById(document.Id) ?? throw new InvalidOperationException("Description not found.");
            Console.Clear();
            Console.WriteLine("MANAGE DESCRIPTION BLOCKS");
            Console.WriteLine("-------------------------");
            Console.WriteLine($"Node:     {node.Title}");
            Console.WriteLine($"Document: {document.Id}");
            Console.WriteLine();

            WriteBlocks(
                document,
                documents
            );

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
                    AddBlock(
                        document,
                        documents,
                        service,
                        node.Id,
                        actorParticipantId
                    );
                    break;
                case "2":
                    UpdateBlock(
                        document,
                        documents,
                        service,
                        node.Id,
                        actorParticipantId
                    );
                    break;
                case "3":
                    ReorderBlock(
                        document,
                        documents,
                        service,
                        node.Id,
                        actorParticipantId
                    );
                    break;
                case "4":
                    RemoveBlock(
                        document,
                        documents,
                        service,
                        node.Id,
                        actorParticipantId
                    );
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
        IDocumentRepository documents
    )
    {
        // The repository resolves IDs in document order, so this list is the
        // exact composition the eventual graphical UI will render.
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

    // Pattern matching keeps console-only rendering outside the Content domain.
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
        IDocumentRepository documents,
        NodeDescriptionService service,
        NodeId nodeId,
        Guid actorId
    )
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

        BlockInput? block = Console.ReadLine() switch
        {
            "1" => NewMarkdown(),
            "2" => NewImage(),
            "3" => NewVideo(),
            "4" => NewLinkPreview(),
            "5" => NewPollReference(),
            "6" => NewChartReference(),
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
            if (!int.TryParse(
                positionText,
                out var position
            ) ||
                position < 1 || position > document.BlockIds.Count + 1)
            {
                ConsoleUi.Pause("That position is not valid.");
                return;
            }

            index = position - 1;
        }

        var added = service.Add(nodeId, actorId, block, index);
        ConsoleUi.Pause($"Added {added.Kind} block {added.Id}.");
    }

    private static void UpdateBlock(
        Document document,
        IDocumentRepository documents,
        NodeDescriptionService service,
        NodeId nodeId,
        Guid actorId
    )
    {
        var block = SelectBlock(
            document,
            documents,
            "Block to update"
        );
        if (block is null) return;

        BlockInput input;

        switch (block)
        {
            case MarkdownTextBlock text:
                Console.Write("New Markdown: ");
                input = new MarkdownInput(Console.ReadLine() ?? string.Empty);
                break;
            case ImageBlock image:
                Console.Write($"Image URL ({image.Url}): ");
                var imageResource = Keep(
                    Console.ReadLine(),
                    image.Url
                );
                Console.Write($"Alt text ({image.AltText}): ");
                var alt = Keep(
                    Console.ReadLine(),
                    image.AltText
                );
                Console.Write($"Caption ({image.Caption}): ");
                input = new ImageInput(imageResource, alt, Console.ReadLine());
                break;
            case VideoBlock video:
                Console.Write($"Video URL ({video.Url}): ");
                var videoResource = Keep(
                    Console.ReadLine(),
                    video.Url
                );
                Console.Write($"Caption ({video.Caption}): ");
                input = new VideoInput(videoResource, Console.ReadLine());
                break;
            case LinkPreviewBlock link:
                Console.Write($"URL ({link.Url}): ");
                var url = Keep(
                    Console.ReadLine(),
                    link.Url.ToString()
                );
                Console.Write($"Title ({link.Title}): ");
                var title = Keep(
                    Console.ReadLine(),
                    link.Title
                );
                Console.Write($"Description ({link.Description}): ");
                input = new LinkPreviewInput(url, title, Console.ReadLine());
                break;
            case PollReferenceBlock:
                ConsoleUi.Pause("This poll reference has no editable block content yet.");
                return;
            case ChartReferenceBlock chart:
                Console.Write($"Title ({chart.Title}): ");
                input = new ChartInput(Keep(Console.ReadLine(), chart.Title), chart.ChartId);
                break;
            default: return;
        }

        service.Update(nodeId, actorId, block.Id, input);
        ConsoleUi.Pause($"Updated {block.Kind} block; its ID remains {block.Id}.");
    }

    private static void ReorderBlock(
        Document document,
        IDocumentRepository documents,
        NodeDescriptionService service,
        NodeId nodeId,
        Guid actorId
    )
    {
        var block = SelectBlock(
            document,
            documents,
            "Block to move"
        );
        if (block is null) return;

        Console.Write($"New position (1-{document.BlockIds.Count}): ");
        if (!int.TryParse(
            Console.ReadLine(),
            out var position
        ) ||
            position < 1 || position > document.BlockIds.Count)
        {
            ConsoleUi.Pause("That position is not valid.");
            return;
        }

        service.Move(nodeId, actorId, block.Id, position - 1);
        ConsoleUi.Pause($"Moved block {block.Id} to position {position}.");
    }

    private static void RemoveBlock(
        Document document,
        IDocumentRepository documents,
        NodeDescriptionService service,
        NodeId nodeId,
        Guid actorId
    )
    {
        var block = SelectBlock(
            document,
            documents,
            "Block to remove"
        );
        if (block is null) return;

        service.Remove(nodeId, actorId, block.Id);
        ConsoleUi.Pause(
            $"Removed block {block.Id} from the description. Its stored record is retained.");
    }

    private static ContentBlock? SelectBlock(
        Document document,
        IDocumentRepository documents,
        string prompt
    )
    {
        var blocks = documents.GetBlocks(document).ToList();
        if (blocks.Count == 0)
        {
            ConsoleUi.Pause("This description has no blocks.");
            return null;
        }

        Console.Write($"{prompt} (1-{blocks.Count}, 0 cancels): ");
        if (!int.TryParse(
            Console.ReadLine(),
            out var selection
        ) ||
            selection < 0 || selection > blocks.Count)
        {
            ConsoleUi.Pause("That block selection is not valid.");
            return null;
        }

        return selection == 0 ? null : blocks[selection - 1];
    }

    private static MarkdownInput NewMarkdown()
    {
        Console.Write("Markdown: ");
        return new MarkdownInput(Console.ReadLine() ?? string.Empty);
    }
    private static ImageInput NewImage()
    {
        Console.Write("Image URL: "); var url = Console.ReadLine();
        Console.Write("Alternative text: "); var alt = Console.ReadLine();
        Console.Write("Caption (optional): ");
        return new ImageInput(url ?? string.Empty, alt ?? string.Empty, Console.ReadLine());
    }
    private static VideoInput NewVideo()
    {
        Console.Write("Video URL: "); var url = Console.ReadLine();
        Console.Write("Caption (optional): ");
        return new VideoInput(url ?? string.Empty, Console.ReadLine());
    }
    private static LinkPreviewInput NewLinkPreview()
    {
        Console.Write("URL: "); var url = Console.ReadLine();
        Console.Write("Title: "); var title = Console.ReadLine();
        Console.Write("Description (optional): ");
        return new LinkPreviewInput(url ?? string.Empty, title ?? string.Empty, Console.ReadLine());
    }
    private static PollInput NewPollReference() => new();
    private static ChartInput NewChartReference()
    {
        Console.Write("Title (optional): ");
        return new ChartInput(Console.ReadLine());
    }

    private static string Keep(
        string? replacement,
        string current
    ) =>
        string.IsNullOrWhiteSpace(replacement) ? current : replacement;

}
