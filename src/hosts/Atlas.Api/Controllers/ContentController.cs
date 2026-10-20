using Atlas.Application;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Map allowlisted JSON block inputs to Content commands after Graph ownership checks.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/nodes/{node:guid}/description")]
public sealed class ContentController(
    INodeRepository nodes,
    IDocumentRepository documents,
    NodeDescriptionService workflow,
    ModerationService moderation,
    RequestActor actor
) : ControllerBase
{
    /// <summary>Project description blocks only when the caller may see the original content.</summary>
    [HttpGet]
    public object Read(Guid node)
    {
        // Resolve the node and check permission to read its original description.
        var target = nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException();
        if (moderation.Visibility(node).IsHidden
            && !(actor.Id is { } id
                && moderation.CanViewHiddenOriginal(id, target.AuthorId.Value, node)))
            throw new KeyNotFoundException();

        // Resolve the description and project only its supported block fields.
        var document = documents.GetById(new DocumentId(target.DescriptionId.Value)) ?? throw new KeyNotFoundException();
        return documents.GetBlocks(document).Select(View);
    }

    /// <summary>Translate an allowlisted JSON block kind and delegate author-checked insertion.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("blocks")]
    public object Add(Guid node, BlockRequest request) => View(workflow.Add(
        new NodeId(node),
        actor.ParticipantId,
        request.Input(),
        request.Index
    ));

    /// <summary>Update an existing block through the node’s owning description workflow.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("blocks/{block:guid}")]
    public object Update(Guid node, Guid block, BlockRequest request) => View(workflow.Update(
        new NodeId(node),
        actor.ParticipantId,
        new BlockId(block),
        request.Input()
    ));

    /// <summary>Change composition order without changing the block’s payload.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("blocks/{block:guid}/position")]
    public IActionResult Move(
        Guid node,
        Guid block,
        PositionRequest request
    )
    {
        workflow.Move(
            new NodeId(node),
            actor.ParticipantId,
            new BlockId(block),
            request.Index
        );
        return NoContent();
    }

    /// <summary>Remove the block from the description composition; retained payload is domain-owned.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("blocks/{block:guid}")]
    public IActionResult Remove(Guid node, Guid block)
    {
        workflow.Remove(
            new NodeId(node),
            actor.ParticipantId,
            new BlockId(block)
        );
        return NoContent();
    }

    private static object View(ContentBlock b) => new
    {
        id = b.Id.Value,
        b.Kind,
        b.CreatedAt,
        b.UpdatedAt,
        content = b switch
        {
            MarkdownTextBlock m => (object)new
            {
                m.Markdown
            },
            ImageBlock i => new
            {
                i.Url,
                i.AltText,
                i.Caption
            },
            VideoBlock v => new
            {
                v.Url,
                v.Caption
            },
            LinkPreviewBlock l => new
            {
                url = l.Url.ToString(),
                l.Title,
                l.Description
            },
            PollReferenceBlock p => new
            {
                p.PollId
            },
            ChartReferenceBlock c => new
            {
                c.ChartId,
                c.Title
            },
            _ => throw new InvalidOperationException()
        }
    };
}

public sealed record PositionRequest(int Index);

public sealed record BlockRequest(
    string Kind,
    string? Text,
    string? Url,
    string? Title,
    string? Caption,
    int? Index
)
{
    public BlockInput Input() => Kind switch
    {
        "markdown" => new MarkdownInput(Text!),
        "image" => new ImageInput(Url!, Text!, Caption),
        "video" => new VideoInput(Url!, Caption),
        "link-preview" => new LinkPreviewInput(Url!, Title!, Text),
        "poll-reference" => new PollInput(),
        "chart-reference" => new ChartInput(Title),
        _ => throw new ArgumentException()
    };
}
