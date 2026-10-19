using Atlas.Application;
using Atlas.Content.Blocks;
using Atlas.Content.Documents;
using Atlas.Graph.Nodes;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

[ApiController, Route("api/nodes/{node:guid}/description")]
public sealed class ContentController(INodeRepository nodes, IDocumentRepository documents, NodeDescriptionService workflow,
    ModerationService moderation, RequestActor actor) : ControllerBase
{
    [HttpGet]
    public object Read(Guid node)
    {
        var target = nodes.GetById(new NodeId(node)) ?? throw new KeyNotFoundException();
        if (moderation.Visibility(node).IsHidden && !(actor.Id is {} id && moderation.CanViewHiddenOriginal(id, target.AuthorId.Value, node)))
            throw new KeyNotFoundException();
        var document = documents.GetById(new DocumentId(target.DescriptionId.Value)) ?? throw new KeyNotFoundException();
        return documents.GetBlocks(document).Select(View);
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("blocks")]
    public object Add(Guid node, BlockRequest request) => View(workflow.Add(new NodeId(node), actor.ParticipantId, request.Input(), request.Index));
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("blocks/{block:guid}")]
    public object Update(Guid node, Guid block, BlockRequest request) => View(workflow.Update(new NodeId(node), actor.ParticipantId, new BlockId(block), request.Input()));
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("blocks/{block:guid}/position")]
    public IActionResult Move(Guid node, Guid block, PositionRequest request) { workflow.Move(new NodeId(node), actor.ParticipantId, new BlockId(block), request.Index); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("blocks/{block:guid}")]
    public IActionResult Remove(Guid node, Guid block) { workflow.Remove(new NodeId(node), actor.ParticipantId, new BlockId(block)); return NoContent(); }
    private static object View(ContentBlock b) => new { id = b.Id.Value, b.Kind, b.CreatedAt, b.UpdatedAt, content = b switch
    {
        MarkdownTextBlock m => (object)new { m.Markdown }, ImageBlock i => new { i.Url, i.AltText, i.Caption },
        VideoBlock v => new { v.Url, v.Caption }, LinkPreviewBlock l => new { url = l.Url.ToString(), l.Title, l.Description },
        PollReferenceBlock p => new { p.PollId }, ChartReferenceBlock c => new { c.ChartId, c.Title }, _ => throw new InvalidOperationException()
    }};
}
public sealed record PositionRequest(int Index);
public sealed record BlockRequest(string Kind, string? Text, string? Url, string? Title, string? Caption, int? Index)
{
    public BlockInput Input() => Kind switch
    {
        "markdown" => new MarkdownInput(Text!), "image" => new ImageInput(Url!, Text!, Caption),
        "video" => new VideoInput(Url!, Caption), "link-preview" => new LinkPreviewInput(Url!, Title!, Text),
        "poll-reference" => new PollInput(), "chart-reference" => new ChartInput(Title), _ => throw new ArgumentException()
    };
}
