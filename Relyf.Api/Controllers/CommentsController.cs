using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CommentsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = new(StringComparer.OrdinalIgnoreCase) { "Idea", "Project" };

    private readonly ILookupRepository _lookup;
    private readonly ICommentRepository _comments;

    public CommentsController(ILookupRepository lookup, ICommentRepository comments)
    {
        _lookup = lookup;
        _comments = comments;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    public sealed record CreateCommentRequest(string TargetType, int TargetId, string Body);

    // POST /api/comments   (JWT required; uses token userId)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be Idea or Project.");
        if (string.IsNullOrWhiteSpace(req.Body)) return BadRequest("Body is required.");

        // target existence via lookup
        var targetOk = req.TargetType.Equals("Idea", StringComparison.OrdinalIgnoreCase)
            ? await _lookup.IdeaExistsAsync(req.TargetId, ct)
            : await _lookup.ProjectExistsAsync(req.TargetId, ct);
        if (!targetOk) return BadRequest("Target not found.");

        var userId = GetUserId();
        var id = await _comments.CreateAsync(userId, req.TargetType, req.TargetId, req.Body.Trim(), ct);
        // return minimal payload (your original returned EF row)
        return CreatedAtAction(nameof(ListForTarget), new { targetType = req.TargetType, targetId = req.TargetId }, new { commentId = id });
    }

    // GET /api/comments/{targetType}/{targetId}?take=50  (public)
    [HttpGet("{targetType}/{targetId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ListForTarget(string targetType, int targetId, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        take = Math.Clamp(take, 1, 200);

        var list = await _comments.ListForTargetAsync(targetType, targetId, take, ct);
        var shaped = list.Select(c => new { c.CommentId, c.UserId, c.Body, c.CreatedAtUtc });
        return Ok(shaped);
    }

    // DELETE /api/comments/{commentId}  (JWT required; owner only)
    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int commentId, CancellationToken ct)
    {
        var userId = GetUserId();
        var n = await _comments.DeleteIfOwnerAsync(commentId, userId, ct);
        return n == 0 ? NotFound() : NoContent();
    }
}
