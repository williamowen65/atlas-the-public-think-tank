using System.Globalization;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Atlas.Api;

internal abstract class ShareImageCache
{
    private const string ContentType = "image/png";
    private const string CaptureTime = "captured_at";

    internal abstract Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken);
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
        internal override Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken)
        {
            var path = Path.Combine(directory, key);
            return Task.FromResult<IResult?>(File.Exists(path) && IsCurrent(File.GetLastWriteTimeUtc(path), revision)
                ? Results.File(path, ContentType)
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
        internal override async Task<IResult?> Read(string key, DateTimeOffset revision, CancellationToken cancellationToken)
        {
            try
            {
                var download = (await container.GetBlobClient(key).DownloadStreamingAsync(
                    cancellationToken: cancellationToken)).Value;
                if (IsCurrent(ReadCaptureTime(download.Details.Metadata), revision))
                    return Results.Stream(download.Content, ContentType);

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
                    HttpHeaders = new BlobHttpHeaders { ContentType = ContentType },
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
