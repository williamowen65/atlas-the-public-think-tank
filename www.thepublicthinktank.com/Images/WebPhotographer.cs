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
            await context.AddInitScriptAsync(scriptPath: scriptPath);

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
                selector = $".card.{contentType}-card";

                await page.GotoAsync(baseUrl + $"/{contentType}/" + imageName);

                await SetColorThemeDark(page);

                // For content cards
                await page.EvalOnSelectorAsync(selector, @"el => { el.style.width = '400px'; el.style.height = '240px'; }");
               
            }

            if (contentType == "homePage")
            {
                selector = "body";
                await page.GotoAsync(baseUrl);
                await SetColorThemeDark(page);
                await page.EvaluateAsync("PrepHomePage()");
                //await page.EvaluateAsync("console.log('hi from playwright')");
            }


            return selector;

        }
    }
}
