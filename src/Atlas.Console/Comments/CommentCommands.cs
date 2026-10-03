using Atlas.Comments.Comments;
using Atlas.Identity;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Comments;

/// <summary>Runs the threaded comment workflow for one discussion target.</summary>
public static class CommentCommands
{
    public static void Run(
        CommentTarget target,
        CommentService service,
        IParticipantRepository participants,
        IAuthenticatedActor authenticatedActor,
        Action<Comment>? onCreated = null)
    {
        while (true)
        {
            var thread = service.GetThread(target);
            WriteThread(thread, participants, authenticatedActor.ParticipantId);

            Console.WriteLine();
            Console.WriteLine("1. Add comment");
            Console.WriteLine("2. Select comment");
            Console.WriteLine("0. Return to node");
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddTopLevel(target, service, authenticatedActor, onCreated);
                    break;
                case "2":
                    Select(thread, service, participants, authenticatedActor, onCreated);
                    break;
                case "0":
                    return;
                default:
                    ConsoleUi.Pause("That is not a valid selection.");
                    break;
            }
        }
    }

    private static void WriteThread(
        IReadOnlyList<CommentThreadItem> thread,
        IParticipantRepository participants,
        Guid actorParticipantId)
    {
        Console.Clear();
        Console.WriteLine("COMMENTS");
        Console.WriteLine("--------");
        var actorDisplayName = participants
            .GetById(new ParticipantId(actorParticipantId))
            ?.DisplayName
            ?? "authenticated participant";
        Console.WriteLine($"Viewing as: {actorDisplayName}");
        Console.WriteLine();

        if (thread.Count == 0)
        {
            Console.WriteLine("No comments yet.");
            return;
        }

        for (var index = 0; index < thread.Count; index++)
        {
            var item = thread[index];
            var comment = item.Comment;
            var indent = new string(' ', item.Depth * 3);
            var author = participants
                .GetById(new ParticipantId(comment.AuthorParticipantId))
                ?.DisplayName ?? "Unknown participant";
            var body = comment.IsRemoved
                ? $"[removed: {FormatStatus(comment.Status)}]"
                : comment.Body;
            var ownComment = comment.AuthorParticipantId == actorParticipantId
                ? " (yours)"
                : string.Empty;

            Console.WriteLine($"{indent}{index + 1}. {author}{ownComment}: {body}");
            Console.WriteLine($"{indent}   {comment.CreatedAt.LocalDateTime:g}");
        }
    }

    public static void AddTopLevel(
        CommentTarget target,
        CommentService service,
        IAuthenticatedActor authenticatedActor, Action<Comment>? onCreated = null)
    {
        Console.Write("Comment: ");
        var body = Console.ReadLine() ?? string.Empty;
        TryChange(
            () => { var created = service.AddTopLevel(
                target,
                authenticatedActor.ParticipantId,
                body,
                DateTimeOffset.UtcNow); onCreated?.Invoke(created); },
            "Comment added.");
    }

    private static void Select(
        IReadOnlyList<CommentThreadItem> thread,
        CommentService service,
        IParticipantRepository participants,
        IAuthenticatedActor authenticatedActor, Action<Comment>? onCreated)
    {
        if (thread.Count == 0)
        {
            ConsoleUi.Pause("There are no comments to select.");
            return;
        }

        Console.Write("Comment number (0 to cancel): ");
        if (!int.TryParse(Console.ReadLine(), out var selection) ||
            selection < 0 || selection > thread.Count)
        {
            ConsoleUi.Pause("That is not a valid comment number.");
            return;
        }

        if (selection == 0) return;

        var comment = thread[selection - 1].Comment;
        while (true)
        {
            WriteSelected(comment, participants, authenticatedActor.ParticipantId);
            var isAuthor = comment.AuthorParticipantId == authenticatedActor.ParticipantId;
            var canChange = isAuthor && !comment.IsRemoved;

            Console.WriteLine("1. Reply");
            Console.WriteLine(canChange ? "2. Edit" : "2. Edit [disabled — requires active comment author]");
            Console.WriteLine(canChange ? "3. Remove" : "3. Remove [disabled — requires active comment author]");
            Console.WriteLine("0. Return to comments");
            Console.Write("Selection: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.Write("Reply: ");
                    var reply = Console.ReadLine() ?? string.Empty;
                    TryChange(
                        () => { var created = service.Reply(comment.Id, authenticatedActor.ParticipantId, reply, DateTimeOffset.UtcNow);
                            onCreated?.Invoke(created); },
                        "Reply added.");
                    return;
                case "2" when canChange:
                    Console.Write("New comment text: ");
                    var body = Console.ReadLine() ?? string.Empty;
                    TryChange(
                        () => service.Edit(comment.Id, authenticatedActor.ParticipantId, body, DateTimeOffset.UtcNow),
                        "Comment updated.");
                    return;
                case "3" when canChange:
                    Console.Write("Remove this comment? (y/n): ");
                    if (string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase))
                    {
                        TryChange(
                            () => service.Remove(comment.Id, authenticatedActor.ParticipantId, DateTimeOffset.UtcNow),
                            "Comment removed.");
                    }
                    return;
                case "2" or "3":
                    ConsoleUi.Pause("That action requires the author of an active comment.");
                    break;
                case "0":
                    return;
                default:
                    ConsoleUi.Pause("That is not a valid selection.");
                    break;
            }
        }
    }

    private static void WriteSelected(
        Comment comment,
        IParticipantRepository participants,
        Guid actorParticipantId)
    {
        Console.Clear();
        var author = participants
            .GetById(new ParticipantId(comment.AuthorParticipantId))
            ?.DisplayName ?? "Unknown participant";
        Console.WriteLine("COMMENT");
        Console.WriteLine("-------");
        Console.WriteLine($"Author: {author}");
        Console.WriteLine($"Status: {FormatStatus(comment.Status)}");
        Console.WriteLine($"Created: {comment.CreatedAt.LocalDateTime:g}");
        var actorDisplayName = participants
            .GetById(new ParticipantId(actorParticipantId))
            ?.DisplayName
            ?? "authenticated participant";
        Console.WriteLine($"Viewing as: {actorDisplayName}");
        Console.WriteLine();
        Console.WriteLine(comment.IsRemoved ? "[comment removed]" : comment.Body);
        Console.WriteLine();
    }

    private static string FormatStatus(CommentStatus status) => status switch
    {
        CommentStatus.Active => "Active",
        CommentStatus.RemovedByAuthor => "Removed by author",
        CommentStatus.RemovedByModerator => "Removed by moderator",
        _ => status.ToString()
    };

    private static void TryChange(Action action, string successMessage)
    {
        try
        {
            action();
            ConsoleUi.Pause(successMessage);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or UnauthorizedAccessException)
        {
            ConsoleUi.Pause($"Unable to update comments: {exception.Message}");
        }
    }
}
