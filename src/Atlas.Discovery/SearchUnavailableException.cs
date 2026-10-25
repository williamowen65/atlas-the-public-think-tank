namespace Atlas.Discovery;

/// <summary>A text-search dependency failed; results must not be presented as empty.</summary>
public sealed class SearchUnavailableException(Exception innerException)
    : Exception("Text search is temporarily unavailable. Please try again later.", innerException);
