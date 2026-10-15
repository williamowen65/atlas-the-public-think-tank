using Atlas.Application.Content;
using Atlas.Application.Eventing;

namespace Atlas.ConsoleApp.Diagnostics;

/// <summary>Terminal formatting for optional event-dispatch/reference-check notices.</summary>
internal static class ConsoleEventDiagnostics
{
    public static void Write(EventDispatchNotice notice) =>
        Console.WriteLine($"[EVENT BUS] {notice.Stage}: {notice.EventType}; subscribers: {notice.SubscriberCount}.");
    public static void Write(NodeContentObservation notice) =>
        Console.WriteLine($"[ATLAS.CONTENT] Node {notice.NodeId} {notice.Transition}; description {notice.DescriptionId}: {(notice.DescriptionExists ? "confirmed" : "not found")}.");
}
