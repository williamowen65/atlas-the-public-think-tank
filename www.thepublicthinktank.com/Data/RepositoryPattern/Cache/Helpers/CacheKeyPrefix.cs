namespace atlas_the_public_think_tank.Data.RepositoryPattern.Cache.Helpers
{
    public class CacheKeyPrefix
    {
        public static string VoteStats = "vote-stats";
        public static string AppUser = "app-user";
        public static string Breadcrumb = "breadcrumb";

        public static string UserHistory = "user-history";

        public static string Issue = "issue";
        public static string IssueVersionHistory = "issue-version-history";

        public static string Solution = "solution";
        public static string SolutionVersionHistory = "solution-version-history";

        // Feed Ids
        public static string SubIssueOfIssueOrSolutionFeedIds = "sub-issue-feed-ids";
        public static string SolutionsOfIssueFeedIds = "solution-feed-ids";
        public static string MainContentFeedIds = "main-content-feed-ids";
        public static string UserIssuesFeedIds = "user-issue-feed-ids";
        public static string UserSolutionsFeedIds = "user-solution-feed-ids";

        // Content Count
        public static string SubIssueForIssueContentCount = "sub-issue-content-counts";
        public static string UserIssuesContentCount = "user-issue-total-count";
        public static string UserSolutionsContentCount = "user-solution-total-count";
        public static string SubIssueForSolutionContentCount = "sub-issue-content-counts";
        public static string SolutionForIssueContentCount = "solution-content-counts";
        public static string MainPageContentCount = "main-content-total-count";
    }
}
