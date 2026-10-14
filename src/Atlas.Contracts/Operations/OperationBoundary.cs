using Atlas.Contracts.Observability;

namespace Atlas.Contracts.Operations;

/// <summary>Persistence adapters provide an atomic boundary around a complete synchronous use case.</summary>
public interface IOperationBoundary
{
    T Execute<T>(Func<T> operation);
}

public static class OperationBoundary
{
    // In-memory adapters may supply their own boundary; SQL repositories always supply one.
    public static T Execute<T>(
        object repository,
        Func<T> operation
    )
    {
        using var correlation = Telemetry.JoinOperation();
        try { return repository is IOperationBoundary boundary ? boundary.Execute(operation) : operation(); }
        catch (Exception error)
        {
            Telemetry.Failure(EventKind.StorageWrite, error);
            throw;
        }
    }
    public static void Execute(
        object repository,
        Action operation
    ) =>
        Execute(
            repository,
            () => { operation(); return true; }
        );
}

/// <summary>A competing write or storage constraint prevented completion. Retry after reloading state.</summary>
public sealed class OperationConflictException(
    string message,
    Exception innerException
)
    : InvalidOperationException(
        message,
        innerException
    );
