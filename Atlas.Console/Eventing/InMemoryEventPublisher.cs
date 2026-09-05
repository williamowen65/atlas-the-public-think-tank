namespace Atlas.ConsoleApp.Eventing;

public sealed class InMemoryEventPublisher
{
    private readonly Dictionary<Type, List<Action<object>>> _subscribers = [];

    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var eventType = typeof(TEvent);

        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _subscribers[eventType] = handlers;
        }

        handlers.Add(message => handler((TEvent)message));

        Console.WriteLine(
            $"[EVENT BUS] Registered subscriber for {eventType.Name}.");
    }

    public void Publish<TEvent>(TEvent message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var eventType = typeof(TEvent);

        Console.WriteLine();
        Console.WriteLine(
            $"[EVENT BUS] Publishing {eventType.Name} at " +
            $"{DateTimeOffset.UtcNow:O}.");

        if (!_subscribers.TryGetValue(eventType, out var handlers))
        {
            Console.WriteLine(
                $"[EVENT BUS] No subscribers accepted {eventType.Name}.");
            return;
        }

        foreach (var handler in handlers)
        {
            handler(message);
        }

        Console.WriteLine(
            $"[EVENT BUS] Delivered {eventType.Name} to " +
            $"{handlers.Count} subscriber(s).");
    }
}
