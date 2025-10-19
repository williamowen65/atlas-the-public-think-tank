using Microsoft.Playwright;
using System.Buffers.Text;

namespace atlas_the_public_think_tank.Images
{
    public class WebPhotographer
    {
        public async Task TakeElementScreenshotAsync(string baseUrl, string contentType, string imageName, string outputPath)
        {
            string? selector = null;

            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1200, Height = 630 }, // <-- based on the ideal dimensions of og:image
                DeviceScaleFactor = 4, // Increase for sharper images (2 = "retina")
                //ColorScheme = ColorScheme.Dark // <-- this doesn't seem to be working. Instead it need to be set on per page basis 
            });

            // Adds helper methods to get the picture correct
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "js", "Site", "web-photographer.js");
            await context.AddInitScriptAsync(scriptPath: scriptPath); // It doesn't actually add the script, just runs it.

            var page = await context.NewPageAsync();

            selector = await ContentTypeHandler(page, baseUrl, contentType, imageName);

            if (selector == null) {
                await browser.CloseAsync();
                return;
            }

            var element = await page.QuerySelectorAsync(selector!);
            if (element != null)
            {
                await element.ScreenshotAsync(new ElementHandleScreenshotOptions { Path = outputPath + $"\\{imageName}.jpg" });
            }

            await browser.CloseAsync();
        }

        public async Task SetColorThemeDark(IPage page) {
            await page.EvalOnSelectorAsync("html", @"el => el.setAttribute('data-bs-theme', 'dark')");
        }

        public async Task<string?> ContentTypeHandler(IPage page, string baseUrl, string contentType, string imageName) {

            string? selector = null;

            if (contentType == "issue" || contentType == "solution")
            {
                selector = $".photographer-card-wrapper";
                await page.GotoAsync(baseUrl + $"/{contentType}/" + imageName);
                await SetColorThemeDark(page);
                await page.EvaluateAsync("PrepContentCard()");

            }

            if (contentType == "homePage")
            {
                selector = "body";
                await page.GotoAsync(baseUrl);
                await SetColorThemeDark(page);
                await page.EvaluateAsync("PrepHomePage()");
            }


            return selector;

        }
    }
}
