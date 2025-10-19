namespace atlas_the_public_think_tank.Images
{
    public interface IImageProvider
    {
        Task<Stream?> GetImageAsync(string imageName);
        Task SaveImageAsync(string relativePath, Stream imageStream);
    }
}
