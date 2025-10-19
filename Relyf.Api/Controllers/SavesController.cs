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

    public SavesController(ISaveRepository saves, ILookupRepository lookup)
    {
        _saves = saves;
        _lookup = lookup;
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

    // GET /api/saves/user/{userId} -> list saved ideas (must match caller)
    [HttpGet("user/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> ListForUser(int userId, CancellationToken ct)
    {
        // Prevent IDOR: the requested userId must be the token's userId
        if (userId != GetUserId()) return Forbid();

        var list = await _saves.ListForUserAsync(userId, ct);
        return Ok(list);
    }
}
