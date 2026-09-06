using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup.TestingData
{
    public static class Users
    {
       

        /// <remarks>
        /// This pattern directly creates a user in the DB
        /// and not via register process, -- so confirmation email is skipped
        /// </remarks>
        public static AppUser CreateTestUser(ApplicationDbContext db, AppUser user, string password)
        {
            // Hash the password
            var passwordHasher = new PasswordHasher<AppUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, password);

            db.Users.Add(user);

            db.SaveChanges();

            return user;
        }

        public static (AppUser appUser, string password) GetRandomAppUser()
        {
            var guid = Guid.NewGuid();
            var email = $"testuser_{guid}@example.com";
            var password = GenerateRandomPassword(12);

            var user = new AppUser
            {
                Id = guid,
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = false
            };

            return (user, password);
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}|;:,.<>?";

            var random = new Random();
            var chars = new List<char>
            {
                upper[random.Next(upper.Length)],
                lower[random.Next(lower.Length)],
                digits[random.Next(digits.Length)],
                special[random.Next(special.Length)]
            };

            string allChars = upper + lower + digits + special;
            for (int i = chars.Count; i < length; i++)
            {
                chars.Add(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle to avoid predictable positions
            return new string(chars.OrderBy(_ => random.Next()).ToArray());
        }



        /// <summary>
        /// Simulates a user login in the test environment by creating authentication cookies
        /// </summary>
        /// <param name="_env">The test environment instance</param>
        /// <param name="user">The user to login</param>
        /// <returns>The authenticated user</returns>
        public static AppUser LoginUser(TestEnvironment _env, AppUser user)
        {
            // Create proper authentication for test environment

            // First, verify we have a valid user with an ID
            if (user == null || user.Id == Guid.Empty)
            {
                throw new ArgumentException("User must be a valid user with an ID");
            }

            // Set the auth cookie with properly formatted user data
            // The cookie name should match what Identity uses
            var authCookieName = ".AspNetCore.Identity.Application";

            // Create a proper authentication ticket value that includes all required claims
            // Note: In tests, we're using a simplified version that the UserManager can still read
            string userData = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{{\"sub\":\"{user.Id}\",\"name\":\"{user.UserName}\",\"email\":\"{user.Email}\",\"role\":\"User\",\"nbf\":{DateTimeOffset.UtcNow.ToUnixTimeSeconds()},\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()},\"iss\":\"test\",\"aud\":\"test\"}}"
            ));

            // Set the authentication cookie with the proper cookie name
            _env.SetCookie<string>(authCookieName, userData);

            // Also set the .AspNetCore.Session cookie to simulate a session
            _env.SetCookie<string>(".AspNetCore.Session", Guid.NewGuid().ToString());

            // Set an antiforgery cookie for form submissions
            _env.SetCookie<string>(".AspNetCore.Antiforgery", Guid.NewGuid().ToString());

            return user;
        }

        /// <summary>
        /// Logs in a user by sending an actual POST request to the login endpoint
        /// </summary>
        /// <param name="_env">The test environment instance</param>
        /// <param name="email">The user's email</param>
        /// <param name="password">The user's password</param>
        /// <returns>True if login was successful, false otherwise</returns>
        public static async Task<bool> LoginUserViaEndpoint(TestEnvironment _env, string email, string password)
        {
            // The standard Identity login endpoint
            string loginUrl = "/login";

           
            // With the following code:
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Input.EmailOrUserName", email),
                new KeyValuePair<string, string>("Input.Password", password),
                new KeyValuePair<string, string>("Input.RememberMe", "true")
            };

            // You need to fetch the login page first to get the anti-forgery token
            var loginPage = await _env.fetchHTML(loginUrl);
            var antiForgeryToken = loginPage.QuerySelector("input[name='__RequestVerificationToken']")?.GetAttribute("value");

            if (!string.IsNullOrEmpty(antiForgeryToken))
            {
             
                if (!string.IsNullOrEmpty(antiForgeryToken))
                {
                    formData.Add(new KeyValuePair<string, string>("__RequestVerificationToken", antiForgeryToken));
                }
            }

            // Send the login POST request
            var response = await _env.PostFormAsync(loginUrl, formData);

            // in the response headers is "Set-Cookie: .AspNetCore.Identity.Application
            // which is used to remember the logged in user

            // Extract cookies from the response headers
            var cookies = _env.GetResponseCookies(response);

            // Set the authentication cookie in the environment if it exists in the response
            if (cookies.TryGetValue(".AspNetCore.Identity.Application", out var authCookieValue))
            {
                // Get raw cookie value without any processing
                var uri = new Uri(_env._client.BaseAddress, "/");

                // Add cookie directly to cookie container
                var cookie = new Cookie(".AspNetCore.Identity.Application", authCookieValue, "/");
                _env._cookieContainer.Add(uri, cookie);
                return true;
            }
            return false;






            //// Check if login was successful - typically a redirect to home page or return URL
            //return response.IsSuccessStatusCode && (response.StatusCode == HttpStatusCode.Redirect ||
            //       response.RequestMessage.RequestUri.ToString().Contains("Account/Login") == false);
        }
    }
}