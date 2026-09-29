using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;

using CloudTests.TestingSetup.TestingData;
using Microsoft.EntityFrameworkCore;
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
        private HttpClient _client;
        private ApplicationDbContext _db;
        private TestEnvironment _env;



        [TestInitialize]
        public void Setup()
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
        public async Task Test_Environment_SQLServer_Has_FullTextFeature_TurnedOn()
        {
            var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') AS IsFullTextInstalled;";
            var result = await cmd.ExecuteScalarAsync();

            Assert.AreEqual(1, Convert.ToInt32(result), "Full-text search should be installed.");
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
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
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
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");


            // Arrange
            var (user2, password2) = Users.GetRandomAppUser();
            AppUser testUser2 = Users.CreateTestUser(_db, user2, password2);
            bool loginSuccess2 = await Users.LoginUserViaEndpoint(_env, user2.Email!, password2);
            Assert.IsTrue(loginSuccess2, "Login 2 should be successful");

        }
    }
}
