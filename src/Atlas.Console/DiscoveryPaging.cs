using Atlas.Discovery;

namespace Atlas.ConsoleApp;

/// <summary>Shared paging labels and navigation for public and authenticated Discovery.</summary>
internal static class DiscoveryPaging
{
    public static int FirstNumber(DiscoveryPage result) => (result.Page - 1) * result.PageSize + 1;

    public static void WriteSummary(
        DiscoveryPage result,
        int visibleCount
    )
    {
        var pageCount = Math.Max(
            1,
            (int)Math.Ceiling(result.TotalCount / (double)result.PageSize)
        );
        Console.WriteLine($"Page {result.Page} of {pageCount}; showing {visibleCount} of {result.TotalCount} matching nodes.");
    }

    public static void WriteNavigation(DiscoveryPage result)
    {
        if (result.HasNextPage) Console.WriteLine("N: next page.");
        if (result.Page > 1) Console.WriteLine("P: previous page.");
    }

    public static bool TryNavigate(
        string? input,
        DiscoveryPage result,
        ref int page
    )
    {
        // Exact P means previous; P<number> remains available for authenticated reporting.
        if (string.Equals(
            input,
            "n",
            StringComparison.OrdinalIgnoreCase
        ))
        {
            if (result.HasNextPage) page = result.Page + 1;
            else ConsoleUi.Pause("There is no next page for the current filters.");
            return true;
        }
        if (string.Equals(
            input,
            "p",
            StringComparison.OrdinalIgnoreCase
        ))
        {
            if (result.Page > 1) page = result.Page - 1;
            else ConsoleUi.Pause("You are already on the first page.");
            return true;
        }
        return false;
    }
}
