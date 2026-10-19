using System.ComponentModel.DataAnnotations;
using Atlas.Application;
using Atlas.Content.Documents;
using Atlas.Discovery;
using Atlas.Graph.Nodes;
using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

[ApiController, Route("api/nodes")]
public sealed class NodesController(INodeRepository nodes, INodeTypeRepository types, IDocumentRepository documents,
    NodeWorkflowService workflow, IDiscoveryService discovery, ModerationService moderation, RequestActor actor) : ControllerBase
{
    [HttpGet]
    public object Discover([FromQuery] DiscoveryQuery query)
    {
        var page = discovery.DiscoverPage(query);
        return new { page.Items, page.TotalCount, page.Page, page.PageSize, page.HasNextPage };
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpGet("mine")]
    public object Mine([FromQuery] DiscoveryQuery query) => discovery.DiscoverAuthoredPage(query,
        nodes.GetByAuthor(new NodeAuthorId(actor.ParticipantId)).Select(node => node.Id.Value).ToArray());
    [HttpGet("types")]
    public object Types() => types.GetAll().Select(t => new { id = t.Id.Value, t.Name });
    [HttpGet("{id:guid}")]
    public object Get(Guid id) => View(Load(id));
    [Authorize(Policy = AtlasPolicies.Member), HttpPost]
    public IActionResult Create(CreateNodeRequest request)
    {
        var node = workflow.Create(documents, request.Title, request.Markdown, new NodeTypeId(request.TypeId),
            actor.ParticipantId, request.RequestedTypeIds.Select(x => new NodeTypeId(x)),
            request.ParentId is {} parent ? new NodeId(parent) : null);
        return Created($"/api/nodes/{node.Id.Value}", View(node));
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/title")]
    public IActionResult Rename(Guid id, TextRequest request) { workflow.Rename(Load(id), actor.ParticipantId, request.Text); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/archive")]
    public IActionResult Archive(Guid id) { workflow.Archive(Load(id), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("{id:guid}/restore")]
    public IActionResult Restore(Guid id) { workflow.Restore(Load(id), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/parents/{parent:guid}")]
    public IActionResult Attach(Guid id, Guid parent) { workflow.Attach(Load(id), new NodeId(parent), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/parents/{parent:guid}")]
    public IActionResult Detach(Guid id, Guid parent) { workflow.Detach(Load(id), new NodeId(parent), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/type/{type:guid}")]
    public IActionResult Type(Guid id, Guid type) { workflow.ChangeType(Load(id), new NodeTypeId(type), actor.ParticipantId); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("{id:guid}/requested-types/{type:guid}")]
    public IActionResult RequestType(Guid id, Guid type) { workflow.RequestType(Load(id), new NodeTypeId(type), actor.ParticipantId, true); return NoContent(); }
    [Authorize(Policy = AtlasPolicies.Member), HttpDelete("{id:guid}/requested-types/{type:guid}")]
    public IActionResult WithdrawType(Guid id, Guid type) { workflow.RequestType(Load(id), new NodeTypeId(type), actor.ParticipantId, false); return NoContent(); }
    private Node Load(Guid id) => nodes.GetById(new NodeId(id)) ?? throw new KeyNotFoundException();
    private object View(Node node)
    {
        var visibility = moderation.Visibility(node.Id.Value);
        if (visibility.IsHidden && !(actor.Id is {} id && moderation.CanViewHiddenOriginal(id, node.AuthorId.Value, node.Id.Value)))
            return new { id = node.Id.Value, hidden = true, notice = visibility.Notice };
        return new { id = node.Id.Value, title = node.Title.Value, typeId = node.TypeId.Value,
            authorId = node.AuthorId.Value, status = node.Status.ToString(), node.CreatedAt, node.UpdatedAt,
            requestedTypeIds = node.RequestedSubNodeTypes.Select(x => x.TypeId.Value),
            parentIds = node.ParentNodeIds.Select(x => x.Value), descriptionId = node.DescriptionId.Value };
    }
}
public sealed record CreateNodeRequest([Required, MaxLength(500)] string Title, [Required, MaxLength(32000)] string Markdown,
    Guid TypeId, Guid? ParentId, [Required, MaxLength(50)] Guid[] RequestedTypeIds);
public sealed record TextRequest([Required, MaxLength(32000)] string Text);
