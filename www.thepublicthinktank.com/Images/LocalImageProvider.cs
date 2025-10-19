namespace atlas_the_public_think_tank.Images
{

    public class LocalImageProvider : IImageProvider
    {
        /// <summary>
        /// A Local path on my machine, which is not under source control.
        /// Meant to demo external blog storage
        /// </summary>
        private readonly string _basePath = @"C:\development-atlas-public-think-tank-blog-storage";

        public Task<Stream?> GetImageAsync(string relativePath)
        {
            // Sanitize the path to prevent directory traversal
            var safePath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
            if (!safePath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
                return Task.FromResult<Stream?>(null);

            if (!File.Exists(safePath)) return Task.FromResult<Stream?>(null);
            return Task.FromResult<Stream?>(File.OpenRead(safePath));
        }

        public async Task SaveImageAsync(string relativePath, Stream imageStream)
        {
            var safePath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
            if (!safePath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid path.");

            var directory = Path.GetDirectoryName(safePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            using var fileStream = new FileStream(safePath, FileMode.Create, FileAccess.Write);
            await imageStream.CopyToAsync(fileStream);
        }
    }

}
