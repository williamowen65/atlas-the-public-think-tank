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
          
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
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
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

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

        [TestMethod]
        public async Task A_User_Can_Be_Created_LoggedIn_AndThen_Later_Another_User_Can_Be_Created_LoggedIn()
        {
            // Arrange
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);
            // Act - Attempt to login via the endpoint
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);
            // Assert - Verify login was successful
            Assert.IsTrue(loginSuccess, "Login should be successful");


            // Arrange
            string email2 = Users.TestUser2.Email!;
            string password2 = Users.TestUser2Password;
            AppUser testUser2 = Users.CreateTestUser(_db, Users.TestUser2, password2);
            // Act - Attempt to login via the endpoint
            bool loginSuccess2 = await Users.LoginUserViaEndpoint(_env, email2, password2);
            // Assert - Verify login was successful
            Assert.IsTrue(loginSuccess2, "Login should be successful");

        }
    }
}
