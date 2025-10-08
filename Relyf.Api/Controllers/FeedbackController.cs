using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeedbackController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = ["Idea", "Project", "App"];
    private readonly RelyfDbContext _db;
    public FeedbackController(RelyfDbContext db) => _db = db;

    public sealed record SubmitRequest(int UserId, string TargetType, int? TargetId, byte? Rating, string? Notes);

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be Idea, Project, or App.");
        if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct)) return BadRequest("Invalid UserId.");
        if (req.TargetType != "App" && (req.TargetId is null)) return BadRequest("TargetId required for non-App feedback.");
        if (req.Rating is < 1 or > 5) return BadRequest("Rating must be 1..5.");

        // Validate target exists when applicable
        if (req.TargetType == "Idea" && !await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.TargetId, ct))
            return BadRequest("Idea not found.");
        if (req.TargetType == "Project" && !await _db.Projects.AnyAsync(p => p.ProjectId == req.TargetId, ct))
            return BadRequest("Project not found.");

        var fb = new Feedback
        {
            UserId = req.UserId,
            TargetType = req.TargetType,
            TargetId = req.TargetId,
            Rating = req.Rating,
            Notes = req.Notes?.Trim()
        };
        _db.Feedback.Add(fb);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(ListForTarget), new { targetType = req.TargetType, targetId = req.TargetId ?? 0 }, fb);
    }

    // GET /api/feedback/{targetType}/{targetId}
    [HttpGet("{targetType}/{targetId:int}")]
    public async Task<IActionResult> ListForTarget(string targetType, int targetId, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        var list = await _db.Feedback.AsNoTracking()
            .Where(f => f.TargetType == targetType && f.TargetId == targetId)
            .OrderByDescending(f => f.FeedbackId)
            .ToListAsync(ct);
        return Ok(list);
    }

    // GET /api/feedback/{targetType}/{targetId}/summary
    [HttpGet("{targetType}/{targetId:int}/summary")]
    public async Task<IActionResult> Summary(string targetType, int targetId, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        var q = _db.Feedback.AsNoTracking().Where(f => f.TargetType == targetType && f.TargetId == targetId);
        var count = await q.CountAsync(ct);
        var avg = count == 0 ? (double?)null : Math.Round(await q.AverageAsync(f => (double)f.Rating!), 2);
        return Ok(new { targetType, targetId, count, average = avg });
    }
}
