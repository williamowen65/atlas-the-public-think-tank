using Atlas.Graph.Nodes.NodeTypes;
using Atlas.Graph.Reactions;
using Atlas.Identity;
using Atlas.Moderation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Expose type/reaction catalog operations while preserving domain ownership and moderator rules.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/catalog")]
public sealed class CatalogController(
    INodeTypeRepository types,
    IReactionDefinitionRepository reactions,
    ModerationService moderation,
    RequestActor actor
) : ControllerBase
{
    /// <summary>List the node types exposed by the shared catalog.</summary>
    [HttpGet("node-types")]
    public object Types() => types.GetAll().Select(View);

    /// <summary>Create a participant-owned node type using the domain creation service.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("node-types")]
    public object CreateType(TypeRequest r) => View(new NodeTypeCreationService(types).Create(
        r.Name,
        r.Description,
        actor.ParticipantId.ToString(),
        DateTimeOffset.UtcNow,
        r.AutoPluralize
    ));

    /// <summary>Apply name, description and pluralization changes with the actor’s permissions.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("node-types/{id:guid}")]
    public IActionResult UpdateType(Guid id, TypeRequest r)
    {
        // Resolve the stored definition before applying domain permission checks.
        var type = types.GetById(new NodeTypeId(id)) ?? throw new KeyNotFoundException();

        // Use one permission decision and timestamp for the related edits.
        var mod = moderation.CanModerate(actor.ParticipantId);
        var at = DateTimeOffset.UtcNow;

        // Apply each catalog change through the domain’s validation methods.
        type.Rename(
            r.Name,
            actor.ParticipantId.ToString(),
            mod,
            at
        );
        type.ChangeDescription(
            r.Description,
            actor.ParticipantId.ToString(),
            mod,
            at
        );
        type.ChangeAutoPluralize(
            r.AutoPluralize,
            actor.ParticipantId.ToString(),
            mod,
            at
        );

        // Persist the completed definition after its domain checks succeed.
        types.Save(type);
        return NoContent();
    }

    /// <summary>Archive a node type after checking owner or moderator permission.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("node-types/{id:guid}/archive")]
    public IActionResult Archive(Guid id)
    {
        // Resolve the stored definition before applying domain permission checks.
        var type = types.GetById(new NodeTypeId(id)) ?? throw new KeyNotFoundException();
        type.Archive(
            actor.ParticipantId.ToString(),
            moderation.CanModerate(actor.ParticipantId),
            DateTimeOffset.UtcNow
        );

        // Persist the completed definition after its domain checks succeed.
        types.Save(type);
        return NoContent();
    }

    /// <summary>List visible reaction definitions, excluding suppressed entries.</summary>
    [HttpGet("reactions")]
    public object Reactions() => reactions.GetAll().Where(x => !x.IsSuppressed).Select(
        x => new
        {
            id = x.Id.Value,
            x.Text,
            x.Emoji,
            x.Description
        }
    );

    /// <summary>Create and persist a reaction definition attributed to the verified actor.</summary>
    [Authorize(Policy = AtlasPolicies.Member), HttpPost("reactions")]
    public object CreateReaction(ReactionRequest r)
    {
        var reaction = ReactionDefinition.Create(
            r.Text,
            r.Emoji,
            r.Description,
            actor.ParticipantId,
            DateTimeOffset.UtcNow
        );

        // Save the validated definition before reporting success.
        reactions.Save(reaction);
        return new
        {
            id = reaction.Id.Value
        };
    }

    /// <summary>Suppress a reaction definition using the current moderator permission.</summary>
    [Authorize(Policy = AtlasPolicies.GlobalModerator), HttpPost("reactions/{id:guid}/suppress")]
    public IActionResult Suppress(Guid id)
    {
        var reaction = reactions.GetById(new ReactionDefinitionId(id)) ?? throw new KeyNotFoundException();
        reaction.Suppress(
            moderation.CanModerate(actor.ParticipantId),
            DateTimeOffset.UtcNow
        );

        // Save the validated definition before reporting success.
        reactions.Save(reaction);
        return NoContent();
    }

    private static object View(NodeTypeDefinition t) => new
    {
        id = t.Id.Value,
        t.Name,
        t.Description,
        t.OwnerId,
        t.IsSystemDefined,
        t.IsArchived,
        t.AutoPluralize
    };
}

public sealed record TypeRequest(
    string Name,
    string Description,
    bool AutoPluralize = true
);

public sealed record ReactionRequest(
    string Text,
    string Emoji,
    string Description
);
