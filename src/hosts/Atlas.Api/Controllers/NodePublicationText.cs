using System.Text.Json;
using System.Text.RegularExpressions;

namespace Atlas.Api.Controllers;

internal static partial class NodePublicationText
{
    internal const int MinimumLength = 200;
    internal const string MinimumMessage = "Add at least 200 characters of text to publish this node.";

    internal static bool MeetsMinimum(string markdown, string? composition)
    {
        if (composition is not null)
        {
            try
            {
                using var document = JsonDocument.Parse(composition, new JsonDocumentOptions { MaxDepth = 24 });
                markdown = string.Join("\n", document.RootElement.GetProperty("blocks").EnumerateArray()
                    .Where(block => block.GetProperty("kind").GetString() == "text")
                    .Select(block => block.GetProperty("text").GetString()));
            }
            catch (Exception error) when (error is JsonException or InvalidOperationException or KeyNotFoundException)
            {
                throw new ArgumentException("Invalid sectioned composition.", nameof(composition), error);
            }
        }

        var text = PlainText(markdown);
        return text.Length >= MinimumLength && LetterOrDigit().IsMatch(text);
    }

    internal static string PlainText(string markdown)
    {
        var text = ImageMarkup().Replace(markdown, "");
        text = LinkMarkup().Replace(text, "$1");
        text = Formatting().Replace(text, "");
        return Whitespace().Replace(text, " ").Trim();
    }

    [GeneratedRegex(@"!\[[^\[\]\r\n]*\](?:\([^)]*\)|\[[^\[\]\r\n]*\])", RegexOptions.NonBacktracking)]
    private static partial Regex ImageMarkup();

    [GeneratedRegex(@"\[([^\[\]\r\n]+)\]\([^)]*\)", RegexOptions.NonBacktracking)]
    private static partial Regex LinkMarkup();

    [GeneratedRegex("[#*_~`>]", RegexOptions.NonBacktracking)]
    private static partial Regex Formatting();

    [GeneratedRegex(@"[\p{L}\p{N}]", RegexOptions.NonBacktracking)]
    private static partial Regex LetterOrDigit();

    [GeneratedRegex(@"\s+", RegexOptions.NonBacktracking)]
    private static partial Regex Whitespace();
}
