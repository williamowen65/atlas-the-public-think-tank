using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Issue;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Solution;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedUsers.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.SolutionTests
{

    [TestClass]
    public class ContentVoting_Solution_Tests
    {
        private HttpClient _client;
        private ApplicationDbContext _db;
        private TestEnvironment _env;


        [TestInitialize]
        public void Setup()
        {
            // Use the utility class to configure the test environment
            _env = new TestEnvironment();
            _db = _env._db;
            _client = _env._client;
        }


        [TestCleanup]
        public async Task ClassCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }



        public AppUser TestSolutionVote { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "solutionvotetest@example.com",
            NormalizedUserName = "SOLUTIONVOTETEST@EXAMPLE.COM",
            Email = "solutionvotetest@example.com",
            NormalizedEmail = "SOLUTIONVOTETEST@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };
        public string TestSolutionVotePassword = "Password1234!";




        [TestMethod]
        public async Task UnauthorizedVote_Returns_YouMustBeLoggedInToVote()
        {
            string url = "/solution/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedSolutions.SeedSolutionDataContainers[0].solution.SolutionID,
                VoteValue = 1
            };

            // Send the unauthorized request
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Get the response content
            Assert.IsTrue(response.Message.Contains("You must login in order to vote"));
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(11)]
        [DataRow(100)]
        public async Task CheckConstraint_SolutionVote_VoteValue_Range(int voteValue)
        {
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, TestSolutionVote, TestSolutionVotePassword);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestSolutionVote.Email!, TestSolutionVotePassword);


            SolutionVote solutionVote = new SolutionVote
            {
                VoteID = Guid.NewGuid(),
                SolutionID = SeedSolutions.SeedSolutionDataContainers[0].solution.SolutionID,
                UserID = testUser.Id,
                VoteValue = voteValue,
                CreatedAt = new DateTime(2024, 1, 26)
            };

            _db.Add(solutionVote);

            int numberOfUpdatesMade = 0;
            try
            {
                numberOfUpdatesMade = _db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Optionally assert that the exception is due to the check constraint
                Assert.IsTrue(ex.InnerException?.Message.Contains("CK_SolutionVote_VoteValue_Range") ?? false,
                    "Exception should be due to VoteValue check constraint.");
            }
            Assert.IsTrue(numberOfUpdatesMade == 0, "SaveChanges should not succeed with out-of-range VoteValue.");

        }



        [TestMethod]
        public async Task AuthorizedVote_MissingVoteData_Returns_ErrorResponses()
        {

            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/solution/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedSolutions.SeedSolutionDataContainers[0].solution.SolutionID,
            };

            // Send the authorized request with bad data
            var response = await _env.fetchPost<object, object>(url, votePayload);
            var responseString = response.ToString();

            // Get the response content
            Assert.IsTrue(
                responseString.Contains("Invalid vote data") &&
                responseString.Contains("errors") &&
                responseString.Contains("JSON deserialization for type") &&
                responseString.Contains("missing required properties") &&
                responseString.Contains("voteValue") &&
                responseString.Contains("The model field is required")
             );
        }

        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(11)]
        [DataRow(-50)]
        [DataRow(50)]
        public async Task AuthorizedVote_VotesOutOfRange_Returns_ErrorResponses(int voteValue)
        {

            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/solution/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedSolutions.SeedSolutionDataContainers[0].solution.SolutionID,
                VoteValue = voteValue
            };

            // Send the authorized request with bad data
            var response = await _env.fetchPost<object, object>(url, votePayload);
            var responseString = response.ToString();

            // Get the response content
            Assert.IsTrue(
                responseString.Contains("Invalid vote data") &&
                responseString.Contains("errors") &&
                responseString.Contains("VoteValue: must be between 0 and 10 (inclusive)")
             );
        }


        [TestMethod]
        public async Task AuthorizedVote_SuccessfulVote_Returns_SuccessResponse()
        {
            // Arrange
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/solution/vote";
            var votePayload = new
            {
                SeedSolutions.SeedSolutionDataContainers[0].solution.SolutionID,
                VoteValue = 7
            };

            // Act
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.Message.Contains("Vote successfully upserted"));
            Assert.IsTrue(response.Count > 0);
        }

        [TestMethod]
        public async Task AuthorizedVote_InvalidSolutionId_Returns_ErrorResponse()
        {
            // Arrange
            var (user, password) = Users.GetRandomAppUser();
            AppUser testUser = Users.CreateTestUser_ViaDbDirectly(_db, user, password);
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, user.Email!, password);
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/solution/vote";
            var votePayload = new
            {
                SolutionID = Guid.NewGuid(),
                VoteValue = 7
            };

            // Act
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Message.Contains("Solution does not exist by the id"));
        }


    }
}