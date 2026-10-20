using System.ComponentModel.DataAnnotations;
using Atlas.Identity;
using Atlas.Participants.Participants;
using Atlas.Participants.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Atlas.Api.Controllers;

/// <summary>Expose public profiles and the authenticated participant’s self-edit operation.</summary>
// Explicit controller policy is visible to CodeQL and reviewers. GET/HEAD/OPTIONS
// remain readable; writes require the cookie/header token pair.
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/participants")]
public sealed class ParticipantsController(IParticipantRepository participants, RequestActor actor) : ControllerBase
{
    [HttpGet]
    public object Page(int offset = 0, int size = 20)
    {
        if (offset < 0 || size < 1 || size > 50) throw new ArgumentException();
        return participants.GetAll().Where(x => x.IsActive).OrderBy(x => x.Id.Value).Skip(offset).Take(size).Select(View);
    }
    [HttpGet("{id:guid}")]
    public object Get(Guid id)
    {
        var p = participants.GetById(new ParticipantId(id));
        if (p is { IsActive: false, DisplayName: AtlasAccounts.DeletedDisplayName })
            return new { id = p.Id.Value, displayName = AtlasAccounts.DeletedDisplayName, bio = "", deleted = true };
        return p is { IsActive: true } ? View(p) : throw new KeyNotFoundException();
    }
    [Authorize(Policy = AtlasPolicies.Member), HttpPut("me")]
    public object Update(ProfileRequest request) => View(new UpdateParticipantProfile(participants).Execute(
        new ParticipantId(actor.ParticipantId), new ParticipantId(actor.ParticipantId), request.DisplayName, request.Bio, DateTimeOffset.UtcNow));
    private static object View(Participant p) => new { id = p.Id.Value, p.DisplayName, p.Bio, p.CreatedAt, p.UpdatedAt };
}
public sealed record ProfileRequest([Required, MaxLength(100)] string DisplayName, [MaxLength(500)] string Bio);
