using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
 
using CloudTests.TestingSetup.TestingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CloudTests.TestingSetup
{

    [TestClass]
    public class TestEnvironment_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;

        [TestInitialize]
        public async Task Setup()
        {

        }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Use the utility class to configure the test environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }



        [TestMethod]
        public async Task CookiesCanBeSetInATest_ReadOnTheServer_RelayedBackToTheTest()
        {
            // Arrange - Create filter with default settings but customize just the vote count
            var filterSettings = new ContentFilter
            {
                TotalVoteCount = new NullableMaxRangeFilter<int> { Min = 1000, Max = null }
                // All other properties will use their defaults
            };

            // Act - Set the cookie
            _env.SetCookie<ContentFilter>("contentFilter", filterSettings);

            // Debug: Inspect the request cookies using the debug endpoint
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/debug/cookies");
            var debugResponse = await _env._client.SendAsync(requestMessage);
            debugResponse.EnsureSuccessStatusCode();
            var cookieDebug = await debugResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"Debug cookies from server: {cookieDebug}");

            // Deserialize the cookie JSON from the server response
            var serverCookies = JsonSerializer.Deserialize<Dictionary<string, string>>(cookieDebug);

            // Verify the contentFilter cookie was received by the server
            Assert.IsNotNull(serverCookies, "Server should return cookie dictionary");
            Assert.IsTrue(serverCookies.ContainsKey("contentFilter"), "Server should receive the contentFilter cookie");
        }



        [TestMethod]
        public async Task A_User_Can_Login_Via_Helper_Methods()
        {
            // Arrange
            AppUser testUser = Users.CreateTestUser1(_db);
            string email = "testuser@example.com";
            string password = "Password123!";

            // Act - Attempt to login via the endpoint
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);

            // Assert - Verify login was successful
            Assert.IsTrue(loginSuccess, "Login should be successful");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/debug/cookies");
            var debugResponse = await _env._client.SendAsync(requestMessage);
            debugResponse.EnsureSuccessStatusCode();
            var cookieDebug = await debugResponse.Content.ReadAsStringAsync();

            Console.WriteLine($"Debug cookies from server: {cookieDebug}");
        }
    }
}
