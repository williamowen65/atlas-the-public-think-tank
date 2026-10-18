namespace Atlas.Contracts.Events;

/// <summary>Host-composed event dispatch port. Publishing is synchronous and is not durable delivery.</summary>
public interface IEventPublisher
{
    void Publish(object message);
}
