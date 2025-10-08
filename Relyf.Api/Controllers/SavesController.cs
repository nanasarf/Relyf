using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SavesController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public SavesController(RelyfDbContext db) => _db = db;

    public sealed record SaveRequest(int UserId, int IdeaId);

    // PUT /api/saves  -> idempotent save (creates if missing)
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] SaveRequest req, CancellationToken ct)
    {
        var userOk = await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct);
        var ideaOk = await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.IdeaId, ct);
        if (!userOk || !ideaOk) return BadRequest("Invalid UserId or IdeaId.");

        var exists = await _db.SavedIdeas.FindAsync(new object[] { req.UserId, req.IdeaId }, ct);
        if (exists is null)
        {
            _db.SavedIdeas.Add(new SavedIdea { UserId = req.UserId, IdeaId = req.IdeaId, SavedAtUtc = DateTime.UtcNow });
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(ListForUser), new { userId = req.UserId }, null);
        }
        return NoContent(); // already saved
    }

    // DELETE /api/saves  -> unsave
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] SaveRequest req, CancellationToken ct)
    {
        var found = await _db.SavedIdeas.FindAsync(new object[] { req.UserId, req.IdeaId }, ct);
        if (found is null) return NotFound();
        _db.SavedIdeas.Remove(found);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/saves/user/1 -> list saved ideas for a user (lightweight)
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> ListForUser(int userId, CancellationToken ct)
    {
        var list = await _db.SavedIdeas
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Join(_db.AiIdeas, s => s.IdeaId, i => i.IdeaId, (s, i) => new
            {
                i.IdeaId,
                i.Title,
                Preview = i.IdeaText.Length > 140
                    ? i.IdeaText.Substring(0, 140) + "..."
                    : i.IdeaText,
                s.SavedAtUtc
            })
            .OrderByDescending(x => x.SavedAtUtc)
            .ToListAsync(ct);

        return Ok(list);
    }

}
