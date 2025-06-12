using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace CloudTests
{
    public static class PlaywrightFixture
    {
        private static bool _browsersInstalled;

        public static async Task<IBrowser> InitializeBrowserAsync(bool headless = true)
        {
            // Only install browsers once per test run
            if (!_browsersInstalled)
            {
                Console.WriteLine("Installing Playwright browsers...");
                var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", "--with-deps", "chromium" });
                if (exitCode != 0)
                {
                    throw new Exception($"Playwright browser installation failed with exit code: {exitCode}");
                }
                _browsersInstalled = true;
            }

            // Determine if running in CI
            bool isRunningInCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = isRunningInCI || headless,
                Timeout = 30000, // Increase timeout for CI environments
                Args = new[] { "--disable-dev-shm-usage" } // Recommended for Docker/CI environments
            });
            
            return browser;
        }
    }
}