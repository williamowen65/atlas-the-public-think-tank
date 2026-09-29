using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace atlas_the_public_think_tank.Images
{
    public class BlobImageProvider : IImageProvider
    {

        private readonly BlobContainerClient _containerClient;

        public BlobImageProvider(IConfiguration configuration)
        {
            var connectionString = configuration["BLOB_STORAGE_CONNECTION_STRING"];
            var containerName = "images"; // Use your container name
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }
        public async Task<Stream?> GetImageAsync(string imageName)
        {
            var blobClient = _containerClient.GetBlobClient(imageName);
            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            return null;
        }

        public async Task SaveImageAsync(string relativePath, Stream imageStream)
        {
            var blobClient = _containerClient.GetBlobClient(relativePath);
            await blobClient.UploadAsync(imageStream, overwrite: true);
        }
    }
}
