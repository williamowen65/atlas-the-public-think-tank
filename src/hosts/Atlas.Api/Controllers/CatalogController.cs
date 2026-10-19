using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Expose type/reaction catalog operations while preserving domain ownership and moderator rules.</summary>
[ApiController, Route("api/catalog")]
public sealed class CatalogController(INodeTypeRepository types, IReactionDefinitionRepository reactions,
    ModerationService moderation, RequestActor actor) : ControllerBase
{
    [HttpGet("node-types")]
    public object Types() => types.GetAll().Select(View);
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("node-types")]
    public object CreateType(TypeRequest r) => View(new NodeTypeCreationService(types).Create(r.Name, r.Description, actor.ParticipantId.ToString(), DateTimeOffset.UtcNow, r.AutoPluralize));
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("node-types/{id:guid}")]
    public IActionResult UpdateType(Guid id, TypeRequest r)
    {
        var type = types.GetById(new NodeTypeId(id)) ?? throw new KeyNotFoundException();
        var mod = moderation.CanModerate(actor.ParticipantId); var at = DateTimeOffset.UtcNow;
        type.Rename(r.Name, actor.ParticipantId.ToString(), mod, at);
        type.ChangeDescription(r.Description, actor.ParticipantId.ToString(), mod, at);
        type.ChangeAutoPluralize(r.AutoPluralize, actor.ParticipantId.ToString(), mod, at);
        types.Save(type); return NoContent();
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("node-types/{id:guid}/archive")]
    public IActionResult Archive(Guid id)
    {
        var type = types.GetById(new NodeTypeId(id)) ?? throw new KeyNotFoundException();
        type.Archive(actor.ParticipantId.ToString(), moderation.CanModerate(actor.ParticipantId), DateTimeOffset.UtcNow);
        types.Save(type); return NoContent();
    }
    [HttpGet("reactions")]
    public object Reactions() => reactions.GetAll().Where(x => !x.IsSuppressed).Select(x => new { id = x.Id.Value, x.Text, x.Emoji, x.Description });
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("reactions")]
    public object CreateReaction(ReactionRequest r)
    {
        var reaction = ReactionDefinition.Create(r.Text, r.Emoji, r.Description, actor.ParticipantId, DateTimeOffset.UtcNow);
        reactions.Save(reaction); return new { id = reaction.Id.Value };
    }
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("reactions/{id:guid}/suppress")]
    public IActionResult Suppress(Guid id)
    {
        var reaction = reactions.GetById(new ReactionDefinitionId(id)) ?? throw new KeyNotFoundException();
        reaction.Suppress(moderation.CanModerate(actor.ParticipantId), DateTimeOffset.UtcNow); reactions.Save(reaction); return NoContent();
    }
    private static object View(NodeTypeDefinition t) => new { id = t.Id.Value, t.Name, t.Description, t.OwnerId, t.IsSystemDefined, t.IsArchived, t.AutoPluralize };
}
public sealed record TypeRequest(string Name, string Description, bool AutoPluralize = true);
public sealed record ReactionRequest(string Text, string Emoji, string Description);
