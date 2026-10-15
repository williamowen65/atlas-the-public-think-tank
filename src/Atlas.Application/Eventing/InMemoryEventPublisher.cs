using Atlas.Contracts.Events;

namespace Atlas.Application.Eventing;

/// <summary>Reusable synchronous dispatcher. Hosts may observe notices for display; no terminal dependency.</summary>
public sealed class InMemoryEventPublisher(Action<EventDispatchNotice>? observe = null) : IEventPublisher
{
    private readonly Dictionary<Type, List<Action<object>>> _subscribers = [];
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var type = typeof(TEvent);
        if (!_subscribers.TryGetValue(type, out var handlers)) _subscribers[type] = handlers = [];
        handlers.Add(message => handler((TEvent)message));
        observe?.Invoke(new("registered", type.Name, handlers.Count));
    }
    public void Publish(object message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var type = message.GetType();
        observe?.Invoke(new("publishing", type.Name, 0));
        if (!_subscribers.TryGetValue(type, out var handlers))
        {
            observe?.Invoke(new("unhandled", type.Name, 0));
            return;
        }
        foreach (var handler in handlers) handler(message);
        observe?.Invoke(new("delivered", type.Name, handlers.Count));
    }
}

public sealed record EventDispatchNotice(string Stage, string EventType, int SubscriberCount);
