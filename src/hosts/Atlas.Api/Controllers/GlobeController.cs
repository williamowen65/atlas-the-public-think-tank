using System.Security.Cryptography;
using System.Text;
using Atlas.Api.Globe;
using Atlas.Identity;
using Atlas.Persistence.Globe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

[ApiController, Route("api/globe"), AutoValidateAntiforgeryToken]
public sealed class GlobeController(GlobeService globe, RequestActor actor) : ControllerBase
{
    [HttpGet("snapshot")]
    public Task<GlobeView> Snapshot(CancellationToken cancellationToken) => globe.View(
        actor.Id is { } id ? Account(id) : null, cancellationToken);

    [Authorize(Policy = AtlasPolicies.Member), HttpPost("activity")]
    public Task<GlobeView> Activity(CancellationToken cancellationToken) => globe.Activity(Account(actor.ParticipantId), cancellationToken);

    [Authorize(Policy = AtlasPolicies.Member), HttpPut("location")]
    public Task<GlobeView> Location(GlobeLocation input, CancellationToken cancellationToken) => globe.SetLocation(
        Account(actor.ParticipantId), input.Longitude, input.Latitude, cancellationToken);

    private static string Account(Guid id) => "a:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(id.ToString())));
}

public sealed record GlobeLocation(double? Longitude, double? Latitude);
