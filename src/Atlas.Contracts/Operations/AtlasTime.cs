namespace Atlas.Contracts.Operations;

/// <summary>One UTC clock source. An execution scope can inject a controlled clock without changing other requests.</summary>
public static class AtlasTime
{
    private static readonly AsyncLocal<TimeProvider?> Current = new();
    public static DateTimeOffset UtcNow => (Current.Value ?? TimeProvider.System).GetUtcNow().ToUniversalTime();

    public static IDisposable Use(TimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var previous = Current.Value;
        Current.Value = provider;
        return new ClockScope(previous);
    }

    private sealed class ClockScope(TimeProvider? previous) : IDisposable
    {
        public void Dispose() => Current.Value = previous;
    }
}
