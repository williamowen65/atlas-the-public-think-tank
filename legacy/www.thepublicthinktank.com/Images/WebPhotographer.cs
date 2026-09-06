using Microsoft.Playwright;
using System.Buffers.Text;

namespace atlas_the_public_think_tank.Images
{
    public class WebPhotographer
    {
        private readonly IImageProvider _imageProvider;
        public WebPhotographer(IImageProvider imageProvider)
        {
            _imageProvider = imageProvider;
        }

        public async Task TakeElementScreenshotAsync(string baseUrl, string contentType, string imageName, string relativePath)
        {
            string? selector = null;

            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1200, Height = 630 },
                DeviceScaleFactor = 4,
            });

            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "js", "Site", "web-photographer.js");
            await context.AddInitScriptAsync(scriptPath: scriptPath);

            var page = await context.NewPageAsync();

            selector = await ContentTypeHandler(page, baseUrl, contentType, imageName);

            if (selector == null)
            {
                await browser.CloseAsync();
                return;
            }

            var element = await page.QuerySelectorAsync(selector!);
            if (element != null)
            {
                var screenshotBytes = await element.ScreenshotAsync(new ElementHandleScreenshotOptions { Path = null, Type = ScreenshotType.Jpeg, Quality = 90, OmitBackground = false });
                using var imageStream = new MemoryStream(screenshotBytes);
                imageStream.Position = 0;
                await _imageProvider.SaveImageAsync(relativePath, imageStream);
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
