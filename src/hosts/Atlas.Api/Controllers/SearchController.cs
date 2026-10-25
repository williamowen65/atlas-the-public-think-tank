using Atlas.Discovery;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.Api.Controllers;

/// <summary>Anonymous mixed search and bounded navigation to a visible comment's thread.</summary>
[AutoValidateAntiforgeryToken]
[ApiController, Route("api/search")]
public sealed class SearchController(IPublicSearch search) : ControllerBase
{
    [HttpGet]
    public PublicSearchPage Search([FromQuery] DiscoveryQuery query, [FromQuery] string scope = "all") => search.Search(query, scope);

    [HttpGet("/api/nodes/{node:guid}/comments/{comment:guid}/path")]
    public object CommentPath(Guid node, Guid comment) => new { items = search.CommentPath(node, comment).Select(c => new
    {
        c.Id, c.AuthorId, body = c.IsRemoved ? null : c.Body,
        status = c.IsRemoved ? "Removed" : "Active", c.CreatedAt, c.UpdatedAt, parentId = c.ParentCommentId
    }) };
}
