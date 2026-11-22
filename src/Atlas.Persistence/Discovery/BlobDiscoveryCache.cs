using System.Diagnostics;
using System.Text.Json;
using Atlas.Contracts.Observability;
using Atlas.Discovery;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Atlas.Persistence.Discovery;

/// <summary>Optional restart-surviving storage for a small set of public feed pages.</summary>
public sealed class BlobDiscoveryCache(BlobContainerClient container)
{
    private const int MaxBytes = 512 * 1024;
    private static readonly TimeSpan Deadline = TimeSpan.FromMilliseconds(400);
    private long _unavailableSince;

    public static BlobDiscoveryCache ForAzure(Uri containerUri, string clientId) =>
        new(new BlobContainerClient(containerUri, new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId)), ClientOptions()));

    public static BlobDiscoveryCache ForEmulator(string connectionString, string container) =>
        new(new BlobContainerClient(connectionString, container, ClientOptions()));

    private static BlobClientOptions ClientOptions() => new()
    {
        Retry = { MaxRetries = 0, NetworkTimeout = Deadline }
    };

    internal Entry? Read(string key)
    {
        if (BackingOff())
        {
            Telemetry.MeasureDiscovery(DiscoveryPhase.ExternalRead, DiscoveryMeasurementOutcome.Skipped, Stopwatch.GetTimestamp());
            return null;
        }
        var started = Stopwatch.GetTimestamp();
        var outcome = DiscoveryMeasurementOutcome.Miss;
        try
        {
            using var deadline = new CancellationTokenSource(Deadline);
            // Cap the HTTP response before buffering or parsing an unexpected payload.
            var blob = container.GetBlobClient("v1/" + key).DownloadContent(
                new BlobDownloadOptions { Range = new HttpRange(0, MaxBytes + 1) }, deadline.Token).Value;
            if (blob.Content.ToMemory().Length > MaxBytes) return null;
            var entry = blob.Content.ToObjectFromJson<Entry>();
            if (entry?.Page?.Items is null || entry.ExpiresAt <= DateTimeOffset.UtcNow ||
                entry.Page.Items.Any(item => item?.Card is null || item.Card.NodeId != item.NodeId || item.Card.IsHidden))
                return null;
            outcome = DiscoveryMeasurementOutcome.Hit;
            return entry;
        }
        catch (RequestFailedException error) when (error.Status == 404) { return null; }
        catch (Exception error) when (IsStorageFailure(error))
        {
            outcome = DiscoveryMeasurementOutcome.Failed;
            _unavailableSince = Stopwatch.GetTimestamp();
            return null;
        }
        finally
        {
            Telemetry.MeasureDiscovery(DiscoveryPhase.ExternalRead, outcome, started);
        }
    }

    internal void Write(string key, DiscoveryPage page, TimeSpan lifetime)
    {
        if (BackingOff() || page.Items.Any(item => item.Card is null || item.Card.IsHidden)) return;
        var started = Stopwatch.GetTimestamp();
        var outcome = DiscoveryMeasurementOutcome.Completed;
        try
        {
            var payload = BinaryData.FromObjectAsJson(new Entry(DateTimeOffset.UtcNow.Add(lifetime), page));
            if (payload.ToMemory().Length > MaxBytes)
            {
                outcome = DiscoveryMeasurementOutcome.Skipped;
                return;
            }
            using var deadline = new CancellationTokenSource(Deadline);
            container.GetBlobClient("v1/" + key).Upload(payload, overwrite: true, deadline.Token);
        }
        catch (Exception error) when (IsStorageFailure(error))
        {
            outcome = DiscoveryMeasurementOutcome.Failed;
            _unavailableSince = Stopwatch.GetTimestamp();
        }
        finally
        {
            Telemetry.MeasureDiscovery(DiscoveryPhase.ExternalWrite, outcome, started);
        }
    }

    private bool BackingOff() => _unavailableSince != 0 &&
        Stopwatch.GetElapsedTime(_unavailableSince) < TimeSpan.FromSeconds(15);

    private static bool IsStorageFailure(Exception error) =>
        error is RequestFailedException or OperationCanceledException or AuthenticationFailedException or
            HttpRequestException or IOException or JsonException;

    internal sealed record Entry(DateTimeOffset ExpiresAt, DiscoveryPage Page);
}
