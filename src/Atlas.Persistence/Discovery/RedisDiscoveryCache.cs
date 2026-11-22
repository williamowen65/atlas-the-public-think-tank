using System.Diagnostics;
using System.Text.Json;
using Atlas.Contracts.Observability;
using StackExchange.Redis;

namespace Atlas.Persistence.Discovery;

/// <summary>Optional shared cache for revision-keyed public Discovery projections.</summary>
public sealed class RedisDiscoveryCache : IDisposable
{
    private const int MaxEntryBytes = 128 * 1024;
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);
    private readonly ConfigurationOptions _options;
    private ConnectionMultiplexer? _connection;
    private long _unavailableSince;

    public RedisDiscoveryCache(string connectionString)
    {
        _options = ConfigurationOptions.Parse(connectionString);
        _options.AbortOnConnectFail = false;
        _options.ConnectRetry = 0;
        _options.ConnectTimeout = 500;
        _options.SyncTimeout = 500;
        _options.BacklogPolicy = BacklogPolicy.FailFast;
    }

    internal T?[] Read<T>(string[] keys) where T : class
    {
        var results = new T?[keys.Length];
        Measure(DiscoveryPhase.ExternalRead, () =>
        {
            var database = Database();
            var values = keys.Length == 1
                ? new[] { database.StringGet(keys[0]) }
                : database.StringGet(keys.Select(key => (RedisKey)key).ToArray());
            for (var i = 0; i < values.Length; i++)
            {
                var bytes = (byte[]?)values[i];
                if (bytes is { Length: > 0 and <= MaxEntryBytes })
                    results[i] = JsonSerializer.Deserialize<T>(bytes);
            }
            return results.All(value => value is not null)
                ? DiscoveryMeasurementOutcome.Hit
                : DiscoveryMeasurementOutcome.Miss;
        });
        return results;
    }

    internal void Write<T>(string key, T value)
    {
        Measure(DiscoveryPhase.ExternalWrite, () =>
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
            if (bytes.Length > MaxEntryBytes) return DiscoveryMeasurementOutcome.Skipped;
            return Database().StringSet(key, bytes, Lifetime)
                ? DiscoveryMeasurementOutcome.Completed
                : DiscoveryMeasurementOutcome.Failed;
        });
    }

    private IDatabase Database()
    {
        _connection ??= ConnectionMultiplexer.Connect(_options);
        if (!_connection.IsConnected) throw new TimeoutException("Discovery Redis is unavailable.");
        return _connection.GetDatabase();
    }

    private void Measure(DiscoveryPhase phase, Func<DiscoveryMeasurementOutcome> operation)
    {
        var started = Stopwatch.GetTimestamp();
        var outcome = DiscoveryMeasurementOutcome.Skipped;
        try
        {
            if (_unavailableSince != 0 && Stopwatch.GetElapsedTime(_unavailableSince) < TimeSpan.FromSeconds(15))
                return;
            outcome = operation();
        }
        catch (JsonException)
        {
            outcome = DiscoveryMeasurementOutcome.Failed;
        }
        catch (Exception error) when (error is RedisException or TimeoutException or ObjectDisposedException)
        {
            _unavailableSince = Stopwatch.GetTimestamp();
            outcome = DiscoveryMeasurementOutcome.Failed;
        }
        finally
        {
            Telemetry.MeasureDiscovery(phase, outcome, started);
        }
    }

    public void Dispose() => _connection?.Dispose();
}
