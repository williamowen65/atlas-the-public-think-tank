using Microsoft.Playwright;
using NuGet.Common;
using System.Buffers.Text;

namespace atlas_the_public_think_tank.Images
{
    public class WebPhotographer
    {
        private readonly IImageProvider _imageProvider;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public WebPhotographer(IImageProvider imageProvider, IWebHostEnvironment env, IConfiguration configuration)
        {
            _imageProvider = imageProvider;
            _env = env;
            _configuration = configuration;
        }

        public async Task TakeElementScreenshotAsync(string baseUrl, string contentType, string imageName, string relativePath)
        {

            string? selector = null;

            using var playwright = await Playwright.CreateAsync();

            IBrowser browser = null;

            if (_env.IsDevelopment())
            {
                // In development
                browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            }
            else
            {
                // in production, testing, staging env
                browser = await playwright.Chromium.ConnectOverCDPAsync(
                    $"wss://production-sfo.browserless.io?token={_configuration["BROWSERLESS_IO_API_KEY"]}"
                );
            }

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
