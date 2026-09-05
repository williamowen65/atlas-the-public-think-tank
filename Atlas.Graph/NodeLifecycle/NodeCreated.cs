public sealed record NodeCreated(
    Guid NodeId,
    string InitialDescription,
    DateTimeOffset OccurredAt);