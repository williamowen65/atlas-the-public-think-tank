
using AngleSharp.Dom;
using atlas_the_public_think_tank.Data.CRUD;
using atlas_the_public_think_tank.Data.DatabaseEntities.Content.Common;
using atlas_the_public_think_tank.Data.DatabaseEntities.Users;
using atlas_the_public_think_tank.Data.DbContext;
using atlas_the_public_think_tank.Data.RepositoryPattern.Repository.Helpers;
using atlas_the_public_think_tank.Data.SeedData.SeedIssues;
using atlas_the_public_think_tank.Data.SeedData.SeedSolutions;

using atlas_the_public_think_tank.Models.ViewModel;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Issue;
using atlas_the_public_think_tank.Models.ViewModel.CRUD.Solution;
using CloudTests.TestingSetup;
using CloudTests.TestingSetup.TestingData;
using System;
using System.Text.Json;


namespace CloudTests.UserTests
{
    [TestClass]
    public class User_UserProfile_Tests
    {
        private static HttpClient _client;
        private static ApplicationDbContext _db;
        private static TestEnvironment _env;
        private TestingCRUDHelper _testingCRUDHelper;


        [TestInitialize]
        public async Task Setup()
        {
            // Use the utility class to configure the test environment
            string appSettings = @"
            {
                ""ApplySeedData"": false,
                ""Caching"": {
                    ""enabled"": true
                }
            }";
            _env = new TestEnvironment(appSettings);
            _db = _env._db;
            _client = _env._client;
            _testingCRUDHelper = new TestingCRUDHelper(_env);

            // Create and login user
            AppUser testUser = Users.CreateTestUser(_db, TestUserProfileUser, TestUserProfilePassword);

            bool loginSuccess = await Users.LoginUserViaEndpoint(_env, TestUserProfileUser.Email!, TestUserProfilePassword);
        }

        public AppUser TestUserProfileUser { get; } = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "userprofile@example.com",
            NormalizedUserName = "USERPROFILE@EXAMPLE.COM",
            Email = "userprofile@example.com",
            NormalizedEmail = "USERPROFILE@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = false
        };
        public string TestUserProfilePassword = "Password1234!";




        [TestCleanup]
        public async Task TestCleanup()
        {
            await TestingUtilityMethods.deleteDatabase(_client, _db);
        }

        public async Task<(string issueId, Scope? scope)> UserCreatesFirstIssue() 
        {
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
            // Visit the user page to populate cache
            var userProfilePage = await _env.fetchHTML(url);

            var userProfileIssueContentCountEl = userProfilePage.QuerySelector("#tab-issue .content-count");
            var userProfileIssueContentCount = int.Parse(userProfileIssueContentCountEl!.InnerHtml);

            Assert.IsTrue(userProfileIssueContentCount == 0, "User issues count should start at 0");

            // Create a new issue

            var testHelper = new TestingCRUDHelper(_env);
            var (jsonDoc, issueId, title, content, scope) = await testHelper.CreateTestIssue();

            // Revisit the user page
            var userProfilePage2 = await _env.fetchHTML(url);

            var userProfileIssueContentCountEl2 = userProfilePage2.QuerySelector("#tab-issue .content-count");
            var userProfileIssueContentCount2 = int.Parse(userProfileIssueContentCountEl2!.InnerHtml);
            Assert.IsTrue(userProfileIssueContentCount2 == 1, "User page issue count should have incremented");
            var userProfileIssue = userProfilePage2.QuerySelector($".card[id='{issueId}']");
            Assert.IsNotNull(userProfileIssue, "New Issue should exist on users page");

            return (issueId, scope);
        }
        public async Task<(string solutionId, Scope? scope, string parentIssueId)> UserCreatesFirstSolution() 
        {
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
  
            // Create a new issue
            var testHelper = new TestingCRUDHelper(_env);
            var (jsonDoc, parentIssueId, title, content, scope) = await testHelper.CreateTestIssue();

            // Visit the user page to populate cache
            var userProfilePage = await _env.fetchHTML(url);

            var userProfileSolutionContentCountEl = userProfilePage.QuerySelector("#tab-solution .content-count");
            var userProfileSolutionContentCount = int.Parse(userProfileSolutionContentCountEl!.InnerHtml);
            Assert.IsTrue(userProfileSolutionContentCount == 0, "User solution count should start at 0");

            var (_jsonDoc, solutionId, solutionTitle, solutionContent, solutionScope) = await testHelper.CreateTestSolution(parentIssueId);

            // Revisit the user page
            var userProfilePage2 = await _env.fetchHTML(url);

            var userProfileSolutionContentCountEl2 = userProfilePage2.QuerySelector("#tab-solution .content-count");
            var userProfileSolutionContentCount2 = int.Parse(userProfileSolutionContentCountEl2!.InnerHtml);
            Assert.IsTrue(userProfileSolutionContentCount2 == 1, "User page solution count should have incremented");
            var userProfileSolution = userProfilePage2.QuerySelector($".card[id='{solutionId}']");
            Assert.IsNotNull(userProfileSolution, "New Solution should exist on users page");


            return (solutionId, solutionScope, parentIssueId);
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenIssueIs_Created()
        {
            // Testing is within this method
            await UserCreatesFirstIssue();
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenIssue_Updated()
        {
            var (issueId, scope) = await UserCreatesFirstIssue();
            var (jsonDoc, _issueId, title, content, _scope) = await _testingCRUDHelper.EditTestIssue(issueId, scope!.ScopeID);
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
            // Visit the user page to populate cache
            var userProfilePage = await _env.fetchHTML(url);

            var userProfileIssueContentCountEl = userProfilePage.QuerySelector("#tab-issue .content-count");
            var userProfileIssueContentCount = int.Parse(userProfileIssueContentCountEl!.InnerHtml);
            Assert.IsTrue(userProfileIssueContentCount == 1, "User page issue count should have stayed the same");
            var userProfileIssue = userProfilePage.QuerySelector($".card[id='{issueId}']");
            var issueTitle = userProfileIssue!.QuerySelector(".card-title");
            // Assert that the issue title/content has been updated
            Assert.IsTrue(issueTitle!.TextContent == title, "Issue title has been updated");

            // Note: The Issue is stored in its own cache which was updated when the issue was edited
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenSolutionIs_Created()
        {
            // testing is within this method
            await UserCreatesFirstSolution();
        }

        [TestMethod]
        public async Task UserProfile_GetsUpdatedWhenSolution_Updated()
        {
            var (solutionId, scope, parentIssueId) = await UserCreatesFirstSolution();
            var (jsonDoc, _issueId, title, content, _scope) = await _testingCRUDHelper.EditTestSolution(solutionId, scope!.ScopeID, parentIssueId);
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
            // Visit the user page to populate cache
            var userProfilePage = await _env.fetchHTML(url);

            var userProfileSolutionContentCountEl = userProfilePage.QuerySelector("#tab-solution .content-count");
            var userProfileSolutionContentCount = int.Parse(userProfileSolutionContentCountEl!.InnerHtml);
            Assert.IsTrue(userProfileSolutionContentCount == 1, "User page solution count should have stayed the same");
            var userProfileSolution = userProfilePage.QuerySelector($".card[id='{solutionId}']");
            var solutionTitle = userProfileSolution!.QuerySelector(".card-title");
            // Assert that the issue title/content has been updated
            Assert.IsTrue(solutionTitle!.TextContent == title, "Solution title has been updated");
            // Note: The Solution is stored in its own cache which was updated when the solution was edited
        }

        [DataTestMethod]
        [DataRow(0, 10)]
        [DataRow(5, 10)]
        [DataRow(1, 4.5)]
        [DataRow(6, 7.8)]
        [DataRow(4.4, 8)]
        [DataRow(2, 9)]
        [DataRow(3, 6.7)]
        public async Task UserProfile_FilterLogic_CanUpdate_UserIssueFeed(double min, double max)
        {
            var (jsonDoc1, issueId1, title1, content1, scope1) = await _testingCRUDHelper.CreateTestIssue();
            var (jsonDoc2, issueId2, title2, content2, scope2) = await _testingCRUDHelper.CreateTestIssue();
            var (jsonDoc3, issueId3, title3, content3, scope3) = await _testingCRUDHelper.CreateTestIssue();
            var (jsonDoc4, issueId4, title4, content4, scope4) = await _testingCRUDHelper.CreateTestIssue();

            await _testingCRUDHelper.CreateTestVoteOnIssue(issueId1, 3);
            await _testingCRUDHelper.CreateTestVoteOnIssue(issueId2, 4);
            await _testingCRUDHelper.CreateTestVoteOnIssue(issueId3, 7);
            await _testingCRUDHelper.CreateTestVoteOnIssue(issueId4, 10);

            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = min, Max = max }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
            var document = await _env.fetchHTML(url);

            var paginationButton = document.QuerySelector("#fetchPaginatedUserIssues");
            var buttonText = paginationButton.TextContent;

            Issue_ReadVM[] AllIssuesOfUser = [
                    await Read.Issue(new Guid(issueId1), new ContentFilter())!,
                    await Read.Issue(new Guid(issueId2), new ContentFilter())!,
                    await Read.Issue(new Guid(issueId3), new ContentFilter())!,
                    await Read.Issue(new Guid(issueId4), new ContentFilter())!,
                ];
            int expectedCount = TestingUtilityMethods.filterByAvgVoteRange(AllIssuesOfUser, min, max).Count();


            string expectedCountDisplay = $"({Math.Min(3, expectedCount)}/{expectedCount})";
            Console.WriteLine($"AverageVote Filter Of {min} to {max}: {expectedCountDisplay}");
            bool passingTest = buttonText.Contains(expectedCountDisplay);
            Assert.IsTrue(passingTest);
        }

        [DataTestMethod]
        [DataRow(0, 10)]
        [DataRow(5, 10)]
        [DataRow(1, 4.5)]
        [DataRow(6, 7.8)]
        [DataRow(4.4, 8)]
        [DataRow(2, 9)]
        [DataRow(3, 6.7)]
        public async Task UserProfile_FilterLogic_CanUpdate_UserSolutionFeed(double min, double max)
        {
            var (_jsonDoc1, parentIssueId1, _title1, _content1, _scope1) = await _testingCRUDHelper.CreateTestIssue();
            var(jsonDoc1, solutionId1, title1, content1, scope1) = await _testingCRUDHelper.CreateTestSolution(parentIssueId1);
            var(jsonDoc2, solutionId2, title2, content2, scope2) = await _testingCRUDHelper.CreateTestSolution(parentIssueId1);
            var(jsonDoc3, solutionId3, title3, content3, scope3) = await _testingCRUDHelper.CreateTestSolution(parentIssueId1);
            var(jsonDoc4, solutionId4, title4, content4, scope4) = await _testingCRUDHelper.CreateTestSolution(parentIssueId1);

            await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId1, 3);
            await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId2, 6);
            await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId3, 3);
            await _testingCRUDHelper.CreateTestVoteOnSolution(solutionId4, 9);

            var filterSettings = new ContentFilter
            {
                AvgVoteRange = new RangeFilter<double> { Min = min, Max = max }
                // All other properties will use their defaults
            };

            _env.SetCookie("contentFilter", filterSettings);
            string url = $"/user-profile?userId={TestUserProfileUser.Id.ToString()}";
            var document = await _env.fetchHTML(url);

            var paginationButton = document.QuerySelector("#fetchPaginatedUserSolutions");
            var buttonText = paginationButton.TextContent;

            Solution_ReadVM[] AllSolutionsOfUser = [
                    await Read.Solution(new Guid(solutionId1), new ContentFilter())!,
                    await Read.Solution(new Guid(solutionId2), new ContentFilter())!,
                    await Read.Solution(new Guid(solutionId3), new ContentFilter())!,
                    await Read.Solution(new Guid(solutionId4), new ContentFilter())!,
                ];
            int expectedCount = TestingUtilityMethods.filterByAvgVoteRange(AllSolutionsOfUser, min, max).Count();


            string expectedCountDisplay = $"({Math.Min(3, expectedCount)}/{expectedCount})";
            Console.WriteLine($"AverageVote Filter Of {min} to {max}: {expectedCountDisplay}");
            bool passingTest = buttonText.Contains(expectedCountDisplay);
            Assert.IsTrue(passingTest);
        }


        //[TestMethod]
        //public async Task UserProfile_UserHistory_IsUpdated_WithEveryAction()
        //{

        //    // Atm 9.21.2025, the content of this section may be updated
        //    // but we can test
        //    Assert.Fail();
        //}



    }
}