
namespace atlas_the_public_think_tank.Images
{
    public class BlobImageProvider : IImageProvider
    {
        public async Task<Stream?> GetImageAsync(string imageName)
        {
            throw new NotImplementedException();
        }

        public async Task SaveImageAsync(string relativePath, Stream imageStream)
        {
            throw new NotImplementedException();
        }
    }
}
