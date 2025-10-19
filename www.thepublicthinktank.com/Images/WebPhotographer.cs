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
                ViewportSize = new ViewportSize { Width = 1200, Height = 800 },
                DeviceScaleFactor = 4 // Increase for sharper images (2 = "retina")
            });

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

        public async Task<string?> ContentTypeHandler(IPage page, string baseUrl, string contentType, string imageName) {

            string? selector = null;

            if (contentType == "issue" || contentType == "solution")
            {
                selector = $".card.{contentType}-card";

                await page.GotoAsync(baseUrl + $"/{contentType}/" + imageName);

                // For content cards
                await page.EvalOnSelectorAsync(selector, @"el => { el.style.width = '400px'; el.style.height = '240px'; }");
               
            }

            if (contentType == "homePage")
            {
                selector = "body";
                await page.GotoAsync(baseUrl);
            }


            return selector;

        }
    }
}
