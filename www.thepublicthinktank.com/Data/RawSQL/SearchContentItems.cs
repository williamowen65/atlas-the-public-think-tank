using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static atlas_the_public_think_tank.Controllers.HomeController;

namespace atlas_the_public_think_tank.Data.RawSQL
{
    public class SearchResult
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rank { get; set; } 

        public string SearchMethod { get; set; }
    }

    public class SearchContentItems
    {
        //private static ApplicationDbContext _context;


        //public static void Initialize(ApplicationDbContext context)
        //{
        //    _context = context;
        //}


        public static async Task<List<SearchResult>> SearchAsync(string searchString, ApplicationDbContext context, double rankCutOffPercent = 0.5, string contentType = "both")
        {

            //double rankCutOffPercent = 0.5;
            if (string.IsNullOrWhiteSpace(searchString))
                return new List<SearchResult>();


            // Query for Issues
            var issuesFreeTextSql = @"
        SELECT 
            i.IssueID, 
            i.Title, 
            i.Content, 
            ft.RANK 
        FROM [issues].[Issues] i
        JOIN FREETEXTTABLE([issues].[Issues], (Title, Content), @p0) ft
            ON i.IssueID = ft.[KEY]";

            var issuesFreeTextQuery = context.Issues
                .FromSqlRaw(issuesFreeTextSql, searchString)
                .Select(i => new SearchResult
                {
                    Id = i.IssueID,
                    Type = "Issue",
                    Title = i.Title,
                    Content = i.Content,
                    Rank = EF.Property<int>(i, "RANK"),
                    SearchMethod = "FreeText"
                });

            // Use prefix search for partial word matching
            var containsSearchString = string.Join(" AND ", searchString
             .Trim()
             .Split(' ', StringSplitOptions.RemoveEmptyEntries)
             .Select(word => $"\"{word}*\""));

            // Issues: CONTAINSTABLE
            var issuesContainsSql = @"
        SELECT 
            i.IssueID, 
            i.Title, 
            i.Content, 
            ct.RANK 
        FROM [issues].[Issues] i
        JOIN CONTAINSTABLE([issues].[Issues], (Title, Content), @p0) ct
            ON i.IssueID = ct.[KEY]";

            var issuesContainsQuery = context.Issues
                .FromSqlRaw(issuesContainsSql, containsSearchString)
                .Select(i => new SearchResult
                {
                    Id = i.IssueID,
                    Type = "Issue",
                    Title = i.Title,
                    Content = i.Content,
                    Rank = EF.Property<int>(i, "RANK"),
                    SearchMethod = "Contains"
                });

            // Solutions: CONTAINSTABLE
            var solutionsContainsSql = @"
            SELECT 
                s.SolutionID, 
                s.Title, 
                s.Content, 
                ct.RANK 
            FROM [solutions].[Solutions] s
            JOIN CONTAINSTABLE([solutions].[Solutions], (Title, Content), @p0) ct
                ON s.SolutionID = ct.[KEY]";

            var solutionsContainsQuery = context.Solutions
                .FromSqlRaw(solutionsContainsSql, containsSearchString)
                .Select(s => new SearchResult
                {
                    Id = s.SolutionID,
                    Type = "Solution",
                    Title = s.Title,
                    Content = s.Content,
                    Rank = EF.Property<int>(s, "RANK"),
                    SearchMethod = "Contains"
                });


            // Query for Solutions
            var solutionsFreeTextSql = @"
        SELECT 
            s.SolutionID, 
            s.Title, 
            s.Content, 
            ft.RANK 
        FROM [solutions].[Solutions] s
        JOIN FREETEXTTABLE([solutions].[Solutions], (Title, Content), @p0) ft
            ON s.SolutionID = ft.[KEY]";

            var solutionsFreeTextQuery = context.Solutions
                .FromSqlRaw(solutionsFreeTextSql, searchString)
                .Select(s => new SearchResult
                {
                    Id = s.SolutionID,
                    Type = "Solution",
                    Title = s.Title,
                    Content = s.Content,
                    Rank = EF.Property<int>(s, "RANK"),
                    SearchMethod = "FreeText"
                });


            IQueryable<SearchResult>? combinedQuery = null;

            if (contentType == "both")
            {
                // Combine, order and take top results
                combinedQuery = issuesContainsQuery.Union(solutionsContainsQuery).Union(issuesFreeTextQuery).Union(solutionsFreeTextQuery);
            }
            else if (contentType == "issue")
            {
                combinedQuery = issuesContainsQuery.Union(issuesFreeTextQuery);
            }
            else if (contentType == "solution")
            {
                combinedQuery = solutionsContainsQuery.Union(solutionsFreeTextQuery);
            } else {
                throw new Exception($"Unknown contentType {contentType}");
            }

            if (!combinedQuery.Any())
                return new List<SearchResult>();

            var maxRank = combinedQuery.Max(q => q.Rank);
            var results = await combinedQuery
                .Where(r => r.Rank > 0)
                .OrderByDescending(r => r.Rank)
                .Take(10)
                .ToListAsync();

            return results
             .GroupBy(r => r.Id)
             .Select(g => g.OrderByDescending(x => x.Rank).First())
             .ToList();
        }


        public static async Task<List<SearchResult>> SearchFreeTextTableAsync(string searchString, ApplicationDbContext context, double rankCutOffPercent = 0.5)
        {

            if (searchString.Trim().Length == 0)
            {
                return new List<SearchResult>();
            }


            // Query for Issues
            var issuesFreeTextSql = @"
        SELECT 
            i.IssueID, 
            i.Title, 
            i.Content, 
            ft.RANK 
        FROM [issues].[Issues] i
        JOIN FREETEXTTABLE([issues].[Issues], (Title, Content), @p0) ft
            ON i.IssueID = ft.[KEY]";

    var issuesFreeTextQuery = context.Issues
        .FromSqlRaw(issuesFreeTextSql, searchString)
        .Select(i => new SearchResult
        {
            Id = i.IssueID,
            Type = "Issue",
            Title = i.Title,
            Content = i.Content,
            Rank = EF.Property<int>(i, "RANK")
        });
        
    // Query for Solutions
    var solutionsFreeTextSql = @"
        SELECT 
            s.SolutionID, 
            s.Title, 
            s.Content, 
            ft.RANK 
        FROM [solutions].[Solutions] s
        JOIN FREETEXTTABLE([solutions].[Solutions], (Title, Content), @p1) ft
            ON s.SolutionID = ft.[KEY]";

    var solutionsFreeTextQuery = context.Solutions
        .FromSqlRaw(solutionsFreeTextSql, searchString)
        .Select(s => new SearchResult
        {
            Id = s.SolutionID,
            Type = "Solution",
            Title = s.Title,
            Content = s.Content,
            Rank = EF.Property<int>(s, "RANK")
        });

            // Combine, order and take top 3
            var combinedQuery = issuesFreeTextQuery.Union(solutionsFreeTextQuery);

            if (combinedQuery.Count() == 0)
            {
                return new List<SearchResult>();
            }

            // DECLARE @maxRank int = (SELECT MAX(RANK) FROM FREETEXTTABLE(...))
            var maxRank = combinedQuery.Max(q => q.Rank);

            return await combinedQuery
                .Where(r => r.Rank > maxRank * rankCutOffPercent)
                .OrderByDescending(r => r.Rank)
                .Take(10)
                .ToListAsync();
        }

        public static async Task<List<SearchResult>> SearchContainsTableAsync(string searchString, ApplicationDbContext context)
        {

            //double rankCutOffPercent = 0.5;
            if (string.IsNullOrWhiteSpace(searchString))
                return new List<SearchResult>();

            // Use prefix search for partial word matching
            var fixedSearchString = string.Join(" OR ", searchString
             .Trim()
             .Split(' ', StringSplitOptions.RemoveEmptyEntries)
             .Select(word => $"\"{word}*\""));

            // Issues: CONTAINSTABLE
            var issuesSql = @"
        SELECT 
            i.IssueID, 
            i.Title, 
            i.Content, 
            ct.RANK 
        FROM [issues].[Issues] i
        JOIN CONTAINSTABLE([issues].[Issues], (Title, Content), @p0) ct
            ON i.IssueID = ct.[KEY]";

            var issuesQuery = context.Issues
                .FromSqlRaw(issuesSql, fixedSearchString)
                .Select(i => new SearchResult
                {
                    Id = i.IssueID,
                    Type = "Issue",
                    Title = i.Title,
                    Content = i.Content,
                    Rank = EF.Property<int>(i, "RANK")
                });

            // Solutions: CONTAINSTABLE
            var solutionsSql = @"
            SELECT 
                s.SolutionID, 
                s.Title, 
                s.Content, 
                ct.RANK 
            FROM [solutions].[Solutions] s
            JOIN CONTAINSTABLE([solutions].[Solutions], (Title, Content), @p1) ct
                ON s.SolutionID = ct.[KEY]";

            var solutionsQuery = context.Solutions
                .FromSqlRaw(solutionsSql, fixedSearchString)
                .Select(s => new SearchResult
                {
                    Id = s.SolutionID,
                    Type = "Solution",
                    Title = s.Title,
                    Content = s.Content,
                    Rank = EF.Property<int>(s, "RANK")
                });

            // Combine, order and take top results
            var combinedQuery = issuesQuery.Union(solutionsQuery);

            if (!combinedQuery.Any())
                return new List<SearchResult>();

            var maxRank = combinedQuery.Max(q => q.Rank);
            return await combinedQuery
                .Where(r => r.Rank > 0)
                .OrderByDescending(r => r.Rank)
                .Take(10)
                .ToListAsync();
        }
   
    
    }
}