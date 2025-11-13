using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SavesController : ControllerBase
{
    private readonly ISaveRepository _saves;
    private readonly ILookupRepository _lookup;
    private readonly IFollowRepository _follows;

    public SavesController(ISaveRepository saves, ILookupRepository lookup, IFollowRepository follows)
    {
        _saves = saves;
        _lookup = lookup;
        _follows = follows;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    public sealed record SaveRequest(int IdeaId);

    // PUT /api/saves  -> idempotent save (creates if missing)
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Put([FromBody] SaveRequest req, CancellationToken ct)
    {
        // Ensure idea exists to avoid FK violations
        if (!await _lookup.IdeaExistsAsync(req.IdeaId, ct))
            return BadRequest("Invalid IdeaId.");

        var userId = GetUserId();
        var inserted = await _saves.PutAsync(userId, req.IdeaId, ct);
        return inserted ? CreatedAtAction(nameof(ListForUser), new { userId }, null) : NoContent();
    }

    // DELETE /api/saves  -> unsave
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete([FromBody] SaveRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        var n = await _saves.DeleteAsync(userId, req.IdeaId, ct);
        return n == 0 ? NotFound() : NoContent();
    }

    // GET /api/saves/user/{userId} -> list saved ideas (own saves or saves of users you follow)
    [HttpGet("user/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> ListForUser(int userId, CancellationToken ct)
    {
        var currentUserId = GetUserId();
        
        // Allow viewing own saves
        if (userId == currentUserId)
        {
            var list = await _saves.ListForUserAsync(userId, ct);
            return Ok(list);
        }
        
        // Allow viewing saves of users you follow
        var isFollowing = await _follows.IsFollowingAsync(currentUserId, userId);
        if (!isFollowing)
        {
            return Forbid();
        }

        var saves = await _saves.ListForUserAsync(userId, ct);
        return Ok(saves);
    }
}
