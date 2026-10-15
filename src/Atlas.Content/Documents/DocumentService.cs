using Atlas.Content.Blocks;

namespace Atlas.Content.Documents;

/// <summary>Content-owned construction and persistence; callers supply one operation timestamp.</summary>
public sealed class DocumentService(IDocumentRepository documents)
{
    // Preparing has no storage side effects, allowing a cross-domain coordinator to
    // validate every proposed aggregate before it starts writing.
    public DocumentDraft PrepareMarkdown(string markdown, DateTimeOffset at)
    {
        var block = new MarkdownTextBlock(markdown, at);
        return new DocumentDraft(new Document([block.Id], at), block);
    }

    public void Save(DocumentDraft draft) => OperationBoundary.Execute(documents, () =>
    {
        ArgumentNullException.ThrowIfNull(draft);
        documents.SaveBlock(draft.Block);
        documents.Save(draft.Document);
    });

    public ContentBlock Add(DocumentId id, BlockInput input, DateTimeOffset at, int? index = null) =>
        OperationBoundary.Execute(documents, () =>
        {
            ArgumentNullException.ThrowIfNull(input);
            var document = Load(id);
            var block = input.Create(at);
            document.AddBlock(block.Id, at, index);
            documents.SaveBlock(block);
            documents.Save(document);
            return block;
        });

    public ContentBlock Update(DocumentId id, BlockId blockId, BlockInput input, DateTimeOffset at) =>
        OperationBoundary.Execute(documents, () =>
        {
            ArgumentNullException.ThrowIfNull(input);
            var document = Load(id);
            if (!document.BlockIds.Contains(blockId))
                throw new InvalidOperationException("The document does not contain this block.");
            var block = documents.GetBlockById(blockId)
                ?? throw new InvalidOperationException("The content block could not be found.");
            input.Apply(block, at);
            documents.SaveBlock(block);
            return block;
        });

    public void Move(DocumentId id, BlockId blockId, int index, DateTimeOffset at) =>
        OperationBoundary.Execute(documents, () =>
        {
            var document = Load(id);
            document.MoveBlock(blockId, index, at);
            documents.Save(document);
        });

    public void Remove(DocumentId id, BlockId blockId, DateTimeOffset at) =>
        OperationBoundary.Execute(documents, () =>
        {
            var document = Load(id);
            document.RemoveBlock(blockId, at);
            // Detach from composition; retain the payload until orphan cleanup is defined.
            documents.Save(document);
        });

    private Document Load(DocumentId id) => documents.GetById(id)
        ?? throw new InvalidOperationException("The description document could not be found.");
}

public sealed record DocumentDraft(Document Document, ContentBlock Block);
