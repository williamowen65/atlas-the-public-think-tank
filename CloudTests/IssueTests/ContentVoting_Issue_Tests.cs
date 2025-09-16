using atlas_the_public_think_tank.Data;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues.Data;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions.Data;
using atlas_the_public_think_tank.Models;
using atlas_the_public_think_tank.Models.ViewModel.AjaxVM;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CloudTests.IssueTests
{

    [TestClass]
    public class ContentVoting_Issue_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;


        [TestInitialize]
        public async Task Setup()
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




        [TestMethod]
        public async Task UnauthorizedVote_Returns_YouMustBeLoggedInToVote()
        {
            string url = "/issue/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedIssues.SeedIssuesDataContainers[0].issue.IssueID,
                VoteValue = 1
            };

            // Send the unauthorized request
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Get the response content
            Assert.IsTrue(response.Message.Contains("You must login in order to vote"));
        }



        [TestMethod]
        public async Task AuthorizedVote_MissingVoteData_Returns_ErrorResponses()
        {

            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);
            Users.LoginUser(_env, testUser);

            string url = "/issue/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedIssues.SeedIssuesDataContainers[0].issue.IssueID,
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

            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);
            Users.LoginUser(_env, testUser);

            string url = "/issue/vote";
            // Create a payload with vote data
            var votePayload = new
            {
                SeedIssues.SeedIssuesDataContainers[0].issue.IssueID,
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
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);
          

            // Act - Attempt to login via the endpoint
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);

            // Assert - Verify login was successful
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/issue/vote";
            var votePayload = new
            {
                SeedIssues.SeedIssuesDataContainers[0].issue.IssueID,
                VoteValue = 7
            };

            // Act
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Assert
            Assert.IsTrue(response.Success);
            Assert.IsTrue(response.Message.Contains("Vote successfully upserted"));
            Assert.IsTrue(response.Count == SeedIssues.SeedIssuesDataContainers[0].issueVotes.Count() + 1);
        }

        [TestMethod]
        public async Task AuthorizedVote_InvalidIssueId_Returns_ErrorResponse()
        {
            // Arrange
            string email = Users.TestUser1.Email!;
            string password = Users.TestUser1Password;
            AppUser testUser = Users.CreateTestUser(_db, Users.TestUser1, password);

            // Act - Attempt to login via the endpoint
            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, email, password);

            // Assert - Verify login was successful
            Assert.IsTrue(loginSuccess, "Login should be successful");

            string url = "/issue/vote";
            var votePayload = new
            {
                IssueID = Guid.NewGuid(),
                VoteValue = 7
            };

            // Act
            var response = await _env.fetchPost<VoteResponse_AjaxVM, object>(url, votePayload);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Message.Contains("Issue does not exist by the id"));
        }


    }
}
