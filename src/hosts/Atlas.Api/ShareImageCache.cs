using System.Globalization;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Atlas.Api;

internal abstract class ShareImageCache
{
    private static string ContentType(string key) => key.EndsWith(".gif", StringComparison.Ordinal) ? "image/gif" : "image/png";
    private const string GlobePointer = "globe-published/current";
    private const string CaptureTime = "captured_at";

    internal abstract Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken);
    internal abstract Task<string?> ReadGlobeVersion(CancellationToken cancellationToken);
    internal abstract Task<bool> PublishGlobeVersion(string version, string? previous);
    internal static bool IsGlobeVersion(string value) => Guid.TryParseExact(value, "N", out _);
    private static bool IsPublishedGlobe(string key) => key.StartsWith("globe-published/", StringComparison.Ordinal);

    internal abstract Task Write(string key, byte[] image, DateTimeOffset capturedAt);

    internal static ShareImageCache? Create(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            var configured = configuration["Atlas:Seo:CaptureDirectory"];
            return new FileCache(Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
                ? "../Atlas.Web/.share-captures/server"
                : configured, environment.ContentRootPath));
        }

        var clientId = configuration["Atlas:Seo:BlobIdentityClientId"];
        if (!Uri.TryCreate(configuration["Atlas:Seo:BlobContainerUrl"], UriKind.Absolute, out var container) ||
            container.Scheme != Uri.UriSchemeHttps || !Guid.TryParse(clientId, out _))
            return null;

        return new BlobCache(new BlobContainerClient(container,
            new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId!)),
            new BlobClientOptions
            {
                Retry = { MaxRetries = 1, NetworkTimeout = TimeSpan.FromSeconds(3) }
            }));
    }

    private static bool IsCurrent(DateTimeOffset capturedAt, DateTimeOffset revision) =>
        capturedAt >= revision && capturedAt >= DateTimeOffset.UtcNow.AddHours(-24);

    private sealed class FileCache(string directory) : ShareImageCache
    {
        internal override async Task<string?> ReadGlobeVersion(CancellationToken cancellationToken)
        {
            var path = Path.Combine(directory, GlobePointer);
            if (!File.Exists(path)) return null;
            var value = await File.ReadAllTextAsync(path, cancellationToken);
            return IsGlobeVersion(value) ? value : null;
        }

        internal override async Task<bool> PublishGlobeVersion(string version, string? previous)
        {
            var path = Path.Combine(directory, GlobePointer);
            await using var publicationLock = File.Open(path + ".lock", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            if (await ReadGlobeVersion(CancellationToken.None) != previous) return false;
            await Write(GlobePointer, System.Text.Encoding.UTF8.GetBytes(version), DateTimeOffset.UtcNow);
            return true;
        }

        internal override Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken)
        {
            var path = Path.Combine(directory, key);
            return Task.FromResult<IResult?>(File.Exists(path) && (IsPublishedGlobe(key) || IsCurrent(File.GetLastWriteTimeUtc(path), revision))
                ? Results.File(path, ContentType(key))
                : null);
        }

        internal override async Task Write(string key, byte[] image, DateTimeOffset capturedAt)
        {
            var path = Path.Combine(directory, key);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllBytesAsync(temporary, image);
                File.SetLastWriteTimeUtc(temporary, capturedAt.UtcDateTime);
                File.Move(temporary, path, overwrite: true);
            }
            finally
            {
                try { File.Delete(temporary); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
            }
        }
    }

    private sealed class BlobCache(BlobContainerClient container) : ShareImageCache
    {
        internal override async Task<string?> ReadGlobeVersion(CancellationToken cancellationToken)
        {
            var pointer = await ReadPointer(cancellationToken);
            var value = pointer?.Content.ToString();
            return value is not null && IsGlobeVersion(value) ? value : null;
        }

        internal override async Task<bool> PublishGlobeVersion(string version, string? previous)
        {
            var pointer = await ReadPointer(CancellationToken.None);
            if (pointer?.Content.ToString() != previous) return false;
            try
            {
                await container.GetBlobClient(GlobePointer).UploadAsync(BinaryData.FromString(version), new BlobUploadOptions
                {
                    Conditions = pointer is null
                        ? new BlobRequestConditions { IfNoneMatch = ETag.All }
                        : new BlobRequestConditions { IfMatch = pointer.Details.ETag }
                });
                return true;
            }
            catch (RequestFailedException error) when (error.Status == 412 ||
                (error.Status == 409 && error.ErrorCode == "BlobAlreadyExists"))
            {
                return false;
            }
        }

        private async Task<BlobDownloadResult?> ReadPointer(CancellationToken cancellationToken)
        {
            try
            {
                return (await container.GetBlobClient(GlobePointer).DownloadContentAsync(cancellationToken)).Value;
            }
            catch (RequestFailedException error) when (error.Status == 404 && error.ErrorCode == "BlobNotFound")
            {
                return null;
            }
        }

        internal override async Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken)
        {
            try
            {
                var download = (await container.GetBlobClient(key).DownloadStreamingAsync(
                    cancellationToken: cancellationToken)).Value;
                if (IsPublishedGlobe(key) || IsCurrent(ReadCaptureTime(download.Details.Metadata), revision))
                    return Results.Stream(download.Content, ContentType(key));

                await download.Content.DisposeAsync();
                return null;
            }
            catch (RequestFailedException error) when (error.Status == 404 && error.ErrorCode == "BlobNotFound")
            {
                return null;
            }
        }

        internal override async Task Write(string key, byte[] image, DateTimeOffset capturedAt)
        {
            var blob = container.GetBlobClient(key);
            var conditions = new BlobRequestConditions { IfNoneMatch = ETag.All };
            try
            {
                var existing = (await blob.GetPropertiesAsync()).Value;
                if (ReadCaptureTime(existing.Metadata) >= capturedAt) return;
                conditions = new BlobRequestConditions { IfMatch = existing.ETag };
            }
            catch (RequestFailedException error) when (error.Status == 404 && error.ErrorCode == "BlobNotFound") { }

            try
            {
                await blob.UploadAsync(BinaryData.FromBytes(image), new BlobUploadOptions
                {
                    Conditions = conditions,
                    HttpHeaders = new BlobHttpHeaders { ContentType = ContentType(key) },
                    Metadata = new Dictionary<string, string>
                    {
                        [CaptureTime] = capturedAt.ToString("O", CultureInfo.InvariantCulture)
                    }
                });
            }
            catch (RequestFailedException error) when (error.Status == 412 ||
                (error.Status == 409 && error.ErrorCode == "BlobAlreadyExists"))
            {
                // Another instance refreshed the same cache entry while this capture was running.
            }
        }

        private static DateTimeOffset ReadCaptureTime(IDictionary<string, string> metadata) =>
            metadata.TryGetValue(CaptureTime, out var stored) &&
            DateTimeOffset.TryParseExact(stored, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out var capturedAt)
                ? capturedAt
                : DateTimeOffset.MinValue;
    }
}
