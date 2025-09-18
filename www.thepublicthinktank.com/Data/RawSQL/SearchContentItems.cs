using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace atlas_the_public_think_tank.Data.RawSQL
{
    public class SearchResult
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rank { get; set; }
    }

    public class SearchContentItems
    {
        //private static ApplicationDbContext _context;

 
        //public static void Initialize(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        public static async Task<List<SearchResult>> SearchAsync(string searchString, ApplicationDbContext context)
{

           
    // Query for Issues
    var issuesSql = @"
        SELECT 
            i.IssueID, 
            i.Title, 
            i.Content, 
            ft.RANK 
        FROM [issues].[Issues] i
        JOIN FREETEXTTABLE([issues].[Issues], (Title, Content), {0}) ft
            ON i.IssueID = ft.[KEY]";

    var issuesQuery = context.Issues
        .FromSqlRaw(issuesSql, searchString)
        .Select(i => new SearchResult
        {
            Id = i.IssueID,
            Type = "Issue",
            Title = i.Title,
            Content = i.Content,
            Rank = EF.Property<int>(i, "RANK")
        });
        
    // Query for Solutions
    var solutionsSql = @"
        SELECT 
            s.SolutionID, 
            s.Title, 
            s.Content, 
            ft.RANK 
        FROM [solutions].[Solutions] s
        JOIN FREETEXTTABLE([solutions].[Solutions], (Title, Content), {0}) ft
            ON s.SolutionID = ft.[KEY]";

    var solutionsQuery = context.Solutions
        .FromSqlRaw(solutionsSql, searchString)
        .Select(s => new SearchResult
        {
            Id = s.SolutionID,
            Type = "Solution",
            Title = s.Title,
            Content = s.Content,
            Rank = EF.Property<int>(s, "RANK")
        });

            // Combine, order and take top 3
            var combinedQuery = issuesQuery.Union(solutionsQuery);

            return await combinedQuery
                .OrderByDescending(r => r.Rank)
                .Take(3)
                .ToListAsync();
        }
    }
}