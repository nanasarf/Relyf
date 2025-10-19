using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReactionsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = new(StringComparer.OrdinalIgnoreCase) { "Idea", "Project" };
    private static readonly HashSet<string> AllowedKinds = new(StringComparer.OrdinalIgnoreCase) { "like", "upvote", "helpful" };

    private readonly ILookupRepository _lookup;
    private readonly IReactionRepository _reactions;

    public ReactionsController(ILookupRepository lookup, IReactionRepository reactions)
    {
        _lookup = lookup;
        _reactions = reactions;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    public sealed record ToggleRequest(string TargetType, int TargetId, string Kind);

    // PUT /api/reactions  -> idempotent add (201 if added, 204 if already existed)
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Put([FromBody] ToggleRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be 'Idea' or 'Project'.");
        if (!AllowedKinds.Contains(req.Kind)) return BadRequest("Kind must be 'like', 'upvote', or 'helpful'.");

        var targetExists = req.TargetType.Equals("Idea", StringComparison.OrdinalIgnoreCase)
            ? await _lookup.IdeaExistsAsync(req.TargetId, ct)
            : await _lookup.ProjectExistsAsync(req.TargetId, ct);
        if (!targetExists) return BadRequest("Target not found.");

        var userId = GetUserId();
        var inserted = await _reactions.PutAsync(userId, req.TargetType, req.TargetId, req.Kind, ct);
        return inserted ? Created() : NoContent();
    }

    // DELETE /api/reactions  -> remove reaction (owner-only via token)
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromBody] ToggleRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType) || !AllowedKinds.Contains(req.Kind))
            return BadRequest("Invalid targetType or kind.");

        var userId = GetUserId();
        var n = await _reactions.DeleteAsync(userId, req.TargetType, req.TargetId, req.Kind, ct);
        return n == 0 ? NotFound() : NoContent();
    }

    // GET /api/reactions/idea/{id}/count?kind=like
    [HttpGet("idea/{id:int}/count")]
    [AllowAnonymous]
    public async Task<IActionResult> CountIdea(int id, [FromQuery] string kind = "like", CancellationToken ct = default)
    {
        if (!AllowedKinds.Contains(kind)) return BadRequest("Invalid kind.");
        var cnt = await _reactions.CountAsync("Idea", id, kind, ct);
        return Ok(new { ideaId = id, kind, count = cnt });
    }

    // GET /api/reactions/project/{id}/count?kind=like
    [HttpGet("project/{id:int}/count")]
    [AllowAnonymous]
    public async Task<IActionResult> CountProject(int id, [FromQuery] string kind = "like", CancellationToken ct = default)
    {
        if (!AllowedKinds.Contains(kind)) return BadRequest("Invalid kind.");
        var cnt = await _reactions.CountAsync("Project", id, kind, ct);
        return Ok(new { projectId = id, kind, count = cnt });
    }
}
