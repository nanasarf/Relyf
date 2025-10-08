using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReactionsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = ["Idea", "Project"];
    private static readonly HashSet<string> AllowedKinds = ["like", "upvote", "helpful"];

    private readonly RelyfDbContext _db;
    public ReactionsController(RelyfDbContext db) => _db = db;

    public sealed record ToggleRequest(int UserId, string TargetType, int TargetId, string Kind);

    // PUT /api/reactions  -> add if missing (idempotent on unique key)
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ToggleRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be 'Idea' or 'Project'.");
        if (!AllowedKinds.Contains(req.Kind)) return BadRequest("Kind must be 'like', 'upvote', or 'helpful'.");

        if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct))
            return BadRequest("UserId does not exist.");

        // Validate the target exists
        var targetExists = req.TargetType == "Idea"
            ? await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.TargetId, ct)
            : await _db.Projects.AnyAsync(p => p.ProjectId == req.TargetId, ct);

        if (!targetExists) return BadRequest("Target not found.");

        var exists = await _db.Reactions.FirstOrDefaultAsync(r =>
            r.UserId == req.UserId && r.TargetType == req.TargetType &&
            r.TargetId == req.TargetId && r.Kind == req.Kind, ct);

        if (exists is null)
        {
            _db.Reactions.Add(new Reaction
            {
                UserId = req.UserId,
                TargetType = req.TargetType,
                TargetId = req.TargetId,
                Kind = req.Kind,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            return Created(); // simple 201
        }
        return NoContent(); // already reacted
    }

    // DELETE /api/reactions  -> remove reaction
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] ToggleRequest req, CancellationToken ct)
    {
        var row = await _db.Reactions.FirstOrDefaultAsync(r =>
            r.UserId == req.UserId && r.TargetType == req.TargetType &&
            r.TargetId == req.TargetId && r.Kind == req.Kind, ct);

        if (row is null) return NotFound();
        _db.Reactions.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/reactions/idea/5/count?kind=like
    [HttpGet("idea/{id:int}/count")]
    public async Task<IActionResult> CountIdea(int id, [FromQuery] string kind = "like", CancellationToken ct = default)
    {
        if (!AllowedKinds.Contains(kind)) return BadRequest("Invalid kind.");
        var cnt = await _db.Reactions.CountAsync(r => r.TargetType == "Idea" && r.TargetId == id && r.Kind == kind, ct);
        return Ok(new { ideaId = id, kind, count = cnt });
    }

    // GET /api/reactions/project/5/count?kind=like
    [HttpGet("project/{id:int}/count")]
    public async Task<IActionResult> CountProject(int id, [FromQuery] string kind = "like", CancellationToken ct = default)
    {
        if (!AllowedKinds.Contains(kind)) return BadRequest("Invalid kind.");
        var cnt = await _db.Reactions.CountAsync(r => r.TargetType == "Project" && r.TargetId == id && r.Kind == kind, ct);
        return Ok(new { projectId = id, kind, count = cnt });
    }
}
