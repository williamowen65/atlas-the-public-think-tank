namespace Atlas.Content.Blocks;

/// <summary>Host-neutral block input. Content owns construction and subtype-safe edits.</summary>
public abstract record BlockInput
{
    internal abstract ContentBlock Create(DateTimeOffset at);
    internal abstract void Apply(ContentBlock block, DateTimeOffset at);
    protected static T Require<T>(ContentBlock block) where T : ContentBlock => block as T
        ?? throw new InvalidOperationException("The input does not match the stored block kind.");
}

public sealed record MarkdownInput(string Markdown) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => new MarkdownTextBlock(Markdown, at);
    internal override void Apply(ContentBlock block, DateTimeOffset at) => Require<MarkdownTextBlock>(block).Update(Markdown, at);
}
public sealed record ImageInput(string Url, string AltText, string? Caption) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => new ImageBlock(Url, AltText, Caption, at);
    internal override void Apply(ContentBlock block, DateTimeOffset at) => Require<ImageBlock>(block).Update(Url, AltText, Caption, at);
}
public sealed record VideoInput(string Url, string? Caption) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => new VideoBlock(Url, Caption, at);
    internal override void Apply(ContentBlock block, DateTimeOffset at) => Require<VideoBlock>(block).Update(Url, Caption, at);
}
public sealed record LinkPreviewInput(string Url, string Title, string? Description) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => new LinkPreviewBlock(Url, Title, Description, at);
    internal override void Apply(ContentBlock block, DateTimeOffset at) => Require<LinkPreviewBlock>(block).UpdatePreview(Url, Title, Description, at);
}
// Null reference IDs preserve today's placeholder creation behavior; they do not create
// real poll/chart entities. Supplying an ID keeps an existing reference on an edit.
public sealed record PollInput(Guid? PollId = null) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => PollId is { } id ? new PollReferenceBlock(id, at) : new PollReferenceBlock(at);
    internal override void Apply(ContentBlock block, DateTimeOffset at) => throw new InvalidOperationException("Poll references have no editable block content yet.");
}
public sealed record ChartInput(string? Title, Guid? ChartId = null) : BlockInput
{
    internal override ContentBlock Create(DateTimeOffset at) => ChartId is { } id ? new ChartReferenceBlock(id, Title, at) : new ChartReferenceBlock(Title, at);
    internal override void Apply(ContentBlock block, DateTimeOffset at)
    {
        var chart = Require<ChartReferenceBlock>(block);
        chart.Update(ChartId ?? chart.ChartId, Title, at);
    }
}
