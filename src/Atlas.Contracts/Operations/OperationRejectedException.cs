using Atlas.Contracts.Observability;

namespace Atlas.Contracts.Operations;

/// <summary>An expected policy rejection, distinct from a broken storage or service dependency.</summary>
/// <remarks>Derived exceptions select a bounded reason; telemetry never serializes the exception message.</remarks>
public abstract class OperationRejectedException(string message) : InvalidOperationException(message)
{
    public abstract EventReason ReasonCode { get; }
}
