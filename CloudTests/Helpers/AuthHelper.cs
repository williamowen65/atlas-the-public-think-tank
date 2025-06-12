using atlas_the_public_think_tank.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Threading.Tasks;

namespace CloudTests.Helpers
{
    public static class AuthHelper
    {
        public static async Task SetupTestUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // Create a test user if it doesn't exist
            var testUser = await userManager.FindByEmailAsync("testuser@example.com");
            if (testUser == null)
            {
                testUser = new AppUser
                {
                    UserName = "testuser@example.com",
                    Email = "testuser@example.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(testUser, "Password123!");
            }
        }

        public static async Task<string> GetLoginCookieAsync(IPage page, string baseUrl)
        {
            // Navigate to login page
            await page.GotoAsync($"{baseUrl}/Identity/Account/Login");

            // Fill and submit the login form
            await page.FillAsync("#Input_Email", "testuser@example.com");
            await page.FillAsync("#Input_Password", "Password123!");
            await page.ClickAsync("#login-submit");

            // Wait for successful login
            await page.WaitForSelectorAsync("text=Logout");

            // Return cookie for future requests
            var cookies = await page.Context.CookiesAsync();
            return cookies.FirstOrDefault(c => c.Name == ".AspNetCore.Identity.Application")?.Value;
        }
    }
}