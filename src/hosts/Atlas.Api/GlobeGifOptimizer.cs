using System.Diagnostics;

namespace Atlas.Api;

internal static class GlobeGifOptimizer
{
    internal static async Task<byte[]> Optimize(byte[] gif)
    {
        var directory = Directory.CreateTempSubdirectory("atlas-globe-gif-");
        try
        {
            var input = Path.Combine(directory.FullName, "input.gif");
            var output = Path.Combine(directory.FullName, "optimized.gif");
            await File.WriteAllBytesAsync(input, gif);

            var options = new ProcessStartInfo("gifsicle") { UseShellExecute = false };
            foreach (var argument in new[] { "-O3", "--colors", "128", "--no-dither", input, "-o", output })
                options.ArgumentList.Add(argument);

            using var process = Process.Start(options)
                ?? throw new InvalidOperationException("Could not start the GIF optimizer.");
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                throw new TimeoutException("The globe GIF optimizer timed out.");
            }

            if (process.ExitCode != 0)
                throw new InvalidOperationException($"The globe GIF optimizer exited with code {process.ExitCode}.");

            return await File.ReadAllBytesAsync(output);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
