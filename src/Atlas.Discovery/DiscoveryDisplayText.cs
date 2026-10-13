namespace Atlas.Discovery;

/// <summary>Shared formatting for immutable Discovery projections and Console cards.</summary>
public static class DiscoveryDisplayText
{
    public static string FormatTypeCount(
        string singularTypeName,
        int count,
        bool autoPluralize
    )
    {
        var displayedName = count == 1
            ? singularTypeName
            : PluralizeTypeName(
                singularTypeName,
                autoPluralize
            );

        return $"{count} {displayedName}";
    }

    /// <summary>Pluralizes a node-type name for count-aware console output.</summary>
    private static string PluralizeTypeName(
        string singularTypeName,
        bool autoPluralize
    )
    {
        if (!autoPluralize)
        {
            return singularTypeName;
        }

        var finalWordStart =
            singularTypeName.LastIndexOf(' ') + 1;
        var prefix = singularTypeName[..finalWordStart];
        var finalWord = singularTypeName[finalWordStart..];

        if (finalWord.EndsWith(
            "y",
            StringComparison.OrdinalIgnoreCase
        ) &&
            finalWord.Length > 1 &&
            !"aeiou".Contains(
                char.ToLowerInvariant(finalWord[^2])))
        {
            return prefix + finalWord[..^1] + "ies";
        }

        if (finalWord.EndsWith(
            "s",
            StringComparison.OrdinalIgnoreCase
        ) ||
            finalWord.EndsWith(
                "x",
                StringComparison.OrdinalIgnoreCase
            ) ||
            finalWord.EndsWith(
                "z",
                StringComparison.OrdinalIgnoreCase
            ) ||
            finalWord.EndsWith(
                "ch",
                StringComparison.OrdinalIgnoreCase
            ) ||
            finalWord.EndsWith(
                "sh",
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return prefix + finalWord + "es";
        }

        return prefix + finalWord + "s";
    }

}
