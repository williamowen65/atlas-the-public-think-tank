using Atlas.Application.Comments;
using Atlas.Comments.Comments;
using Atlas.Contracts.Identity;
using Atlas.Participants.Participants;

namespace Atlas.ConsoleApp.Comments;

/// <summary>Browse one page of roots or direct replies, loading another branch only when selected.</summary>
public static class CommentCommands
{
    public static void Run(CommentTarget target, CommentWorkflowService service,
        IParticipantRepository participants, IAuthenticatedActor authenticatedActor) =>
        Browse(target, request => service.GetPageAsync(request), participants, service, authenticatedActor);

    /// <summary>The anonymous browser uses the same bounded queries without contribution controls.</summary>
    public static void ReadOnly(CommentTarget target, ICommentRepository comments, IParticipantRepository participants) =>
        Browse(target, request => comments.ReadPageAsync(request), participants, null, null);

    private static void Browse(CommentTarget target, Func<CommentPageRequest, Task<CommentPage>> read,
        IParticipantRepository participants, CommentWorkflowService? service, IAuthenticatedActor? actor)
    {
        var location = new CommentPageRequest(target);
        // Store only navigation positions, never previously loaded threads. No recursive UI calls.
        var history = new Stack<CommentPageRequest>();
        while (true)
        {
            var page = read(location).GetAwaiter().GetResult();
            WritePage(page, location.ParentId, participants, actor?.ParticipantId);
            if (actor is not null) Console.WriteLine(location.ParentId is null ? "1. Add comment" : "1. Reply to this parent");
            Console.WriteLine("2. Select comment / view replies");
            if (page.Next is not null) Console.WriteLine("3. Next page");
            if (history.Count > 0) Console.WriteLine("4. Back to previous page or parent");
            Console.WriteLine("0. Return to node");
            Console.Write("Selection: ");
            switch (Console.ReadLine())
            {
                case null or "0": return;
                case "1" when actor is not null:
                    if (location.ParentId is { } parent) Reply(parent, service!, actor);
                    else AddTopLevel(target, service!, actor);
                    break;
                case "2":
                    Console.Write("Comment number (0 to cancel): ");
                    if (!int.TryParse(Console.ReadLine(), out var number) || number < 0 || number > page.Items.Count)
                    {
                        ConsoleUi.Pause("That is not a valid comment number.");
                        break;
                    }
                    if (number == 0) break;
                    var selected = page.Items[number - 1];
                    if (Select(selected, service, participants, actor))
                    {
                        history.Push(location);
                        location = new CommentPageRequest(target, selected.Id);
                    }
                    break;
                case "3" when page.Next is not null:
                    history.Push(location);
                    location = new CommentPageRequest(target, location.ParentId, page.Next);
                    break;
                case "4" when history.Count > 0:
                    location = history.Pop();
                    break;
                default: ConsoleUi.Pause("That is not a valid selection."); break;
            }
        }
    }

    private static void WritePage(CommentPage page, CommentId? parent, IParticipantRepository participants, Guid? actor)
    {
        Console.Clear();
        Console.WriteLine(actor is null ? "COMMENTS (read only)" : "COMMENTS");
        Console.WriteLine(parent is null ? "Top-level comments" : $"Direct replies to {parent}");
        Console.WriteLine("Oldest first; select a comment to open its replies.");
        Console.WriteLine($"Showing {page.Items.Count} comments on this page.");
        if (page.Items.Count == 0) Console.WriteLine(parent is null ? "No comments yet." : "No replies yet.");
        for (var index = 0; index < page.Items.Count; index++)
        {
            var comment = page.Items[index];
            var author = participants.GetById(new ParticipantId(comment.AuthorParticipantId))?.DisplayName ?? "Unknown participant";
            var body = comment.IsRemoved ? $"[removed: {FormatStatus(comment.Status)}]" : comment.Body;
            Console.WriteLine($"{index + 1}. {author}{(comment.AuthorParticipantId == actor ? " (yours)" : "")}: {body}");
            Console.WriteLine($"   {comment.CreatedAt.LocalDateTime:g}");
        }
    }

    // Return a navigation choice rather than recursively opening another browser.
    private static bool Select(Comment comment, CommentWorkflowService? service,
        IParticipantRepository participants, IAuthenticatedActor? actor)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("COMMENT");
            var author = participants.GetById(new ParticipantId(comment.AuthorParticipantId))?.DisplayName ?? "Unknown participant";
            Console.WriteLine($"Author: {author}");
            Console.WriteLine($"Status: {FormatStatus(comment.Status)}");
            Console.WriteLine($"Created: {comment.CreatedAt.LocalDateTime:g}");
            Console.WriteLine(comment.IsRemoved ? $"[removed: {FormatStatus(comment.Status)}]" : comment.Body);
            var canChange = actor is not null && comment.AuthorParticipantId == actor.ParticipantId && !comment.IsRemoved;
            if (actor is not null)
            {
                Console.WriteLine("1. Reply");
                Console.WriteLine(canChange ? "2. Edit" : "2. Edit [disabled — requires active comment author]");
                Console.WriteLine(canChange ? "3. Remove" : "3. Remove [disabled — requires active comment author]");
            }
            Console.WriteLine("4. View direct replies");
            Console.WriteLine("0. Return to comments");
            Console.Write("Selection: ");
            switch (Console.ReadLine())
            {
                case null or "0": return false;
                case "4": return true;
                case "1" when actor is not null:
                    Reply(comment.Id, service!, actor); return false;
                case "2" when canChange:
                    Console.Write("New comment text: ");
                    var body = Console.ReadLine() ?? string.Empty;
                    TryChange(() => service!.Edit(comment.Id, actor!.ParticipantId, body, AtlasTime.UtcNow), "Comment updated.");
                    return false;
                case "3" when canChange:
                    Console.Write("Remove this comment? (y/n): ");
                    if (string.Equals(Console.ReadLine(), "y", StringComparison.OrdinalIgnoreCase))
                        TryChange(() => service!.Remove(comment.Id, actor!.ParticipantId, AtlasTime.UtcNow), "Comment removed.");
                    return false;
                default: ConsoleUi.Pause("That is not a valid selection."); break;
            }
        }
    }

    public static void AddTopLevel(CommentTarget target, CommentWorkflowService service, IAuthenticatedActor actor)
    {
        Console.Write("Comment: ");
        var body = Console.ReadLine() ?? string.Empty;
        TryChange(() => service.AddTopLevel(target, actor.ParticipantId, body, AtlasTime.UtcNow), "Comment added.");
    }
    private static void Reply(CommentId parent, CommentWorkflowService service, IAuthenticatedActor actor)
    {
        Console.Write("Reply: ");
        var body = Console.ReadLine() ?? string.Empty;
        TryChange(() => service.Reply(parent, actor.ParticipantId, body, AtlasTime.UtcNow), "Reply added.");
    }
    private static string FormatStatus(CommentStatus status) => status switch
    {
        CommentStatus.Active => "Active", CommentStatus.RemovedByAuthor => "Removed by author",
        CommentStatus.RemovedByModerator => "Removed by moderator", _ => status.ToString()
    };
    private static void TryChange(Action action, string successMessage)
    {
        try { action(); ConsoleUi.Pause(successMessage); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or UnauthorizedAccessException)
        { ConsoleUi.Pause($"Unable to update comments: {error.Message}"); }
    }
}
