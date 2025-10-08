using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CommentsController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = ["Idea", "Project"];
    private readonly RelyfDbContext _db;
    public CommentsController(RelyfDbContext db) => _db = db;

    public sealed record CreateCommentRequest(int UserId, string TargetType, int TargetId, string Body);

    // POST /api/comments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be Idea or Project.");
        if (string.IsNullOrWhiteSpace(req.Body)) return BadRequest("Body is required.");
        if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct)) return BadRequest("Invalid UserId.");
        var targetOk = req.TargetType == "Idea"
            ? await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.TargetId, ct)
            : await _db.Projects.AnyAsync(p => p.ProjectId == req.TargetId, ct);
        if (!targetOk) return BadRequest("Target not found.");

        var row = new Comment
        {
            UserId = req.UserId,
            TargetType = req.TargetType,
            TargetId = req.TargetId,
            Body = req.Body.Trim()
            // CreatedAtUtc set by DB default
        };
        _db.Comments.Add(row);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(ListForTarget), new { targetType = row.TargetType, targetId = row.TargetId }, row);
    }

    // GET /api/comments/{targetType}/{targetId}?take=50
    [HttpGet("{targetType}/{targetId:int}")]
    public async Task<IActionResult> ListForTarget(string targetType, int targetId, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        take = Math.Clamp(take, 1, 200);

        var list = await _db.Comments.AsNoTracking()
            .Where(c => c.TargetType == targetType && c.TargetId == targetId)
            .OrderByDescending(c => c.CommentId)
            .Take(take)
            .Select(c => new {
                c.CommentId,
                c.UserId,
                c.Body,
                c.CreatedAtUtc
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    // DELETE /api/comments/{commentId}
    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> Delete(int commentId, CancellationToken ct)
    {
        var row = await _db.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId, ct);
        if (row is null) return NotFound();
        _db.Comments.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
