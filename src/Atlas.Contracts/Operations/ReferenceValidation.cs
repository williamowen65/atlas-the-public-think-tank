namespace Atlas.Contracts.Operations;

/// <summary>Host adapter resolves cross-domain existence/activity facts. It does not establish caller identity.</summary>
public interface IReferenceLookup
{
    bool IsAvailable(string kind, Guid id, bool requireActive);
}

public static class ReferenceValidation
{
    public static void Require(object adapter, string kind, Guid id, bool requireActive = true)
    {
        if (id == Guid.Empty) throw new ArgumentException($"A {kind} ID is required.");
        if (adapter is not IReferenceLookup lookup)
            throw new InvalidOperationException("The application requires a reference-availability adapter.");
        if (!lookup.IsAvailable(kind, id, requireActive))
            throw new InvalidOperationException($"The referenced {kind} is missing or unavailable.");
    }
}
