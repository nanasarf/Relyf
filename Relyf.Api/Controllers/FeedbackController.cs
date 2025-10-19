using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using Relyf.Repository.Dapper.Models;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeedbackController : ControllerBase
{
    private static readonly HashSet<string> AllowedTargets = new(StringComparer.OrdinalIgnoreCase) { "Idea", "Project", "App" };

    private readonly IFeedbackRepository _feedback;
    private readonly ILookupRepository _lookup;

    public FeedbackController(IFeedbackRepository feedback, ILookupRepository lookup)
    {
        _feedback = feedback;
        _lookup = lookup;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    public sealed record SubmitRequest(string TargetType, int? TargetId, byte? Rating, string? Notes);

    // POST /api/feedback
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Submit([FromBody] SubmitRequest req, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(req.TargetType)) return BadRequest("TargetType must be Idea, Project, or App.");
        if (req.TargetType != "App" && (req.TargetId is null)) return BadRequest("TargetId required for non-App feedback.");
        if (req.Rating is < 1 or > 5) return BadRequest("Rating must be 1..5.");

        // Validate target existence when applicable
        if (req.TargetType.Equals("Idea", StringComparison.OrdinalIgnoreCase) && !await _lookup.IdeaExistsAsync(req.TargetId!.Value, ct))
            return BadRequest("Idea not found.");
        if (req.TargetType.Equals("Project", StringComparison.OrdinalIgnoreCase) && !await _lookup.ProjectExistsAsync(req.TargetId!.Value, ct))
            return BadRequest("Project not found.");

        var userId = GetUserId();
        var id = await _feedback.CreateAsync(userId, req.TargetType, req.TargetId, req.Rating, req.Notes?.Trim(), ct);

        // For App feedback (no target), the original code used 0 in the route
        var targetIdForRoute = req.TargetId ?? 0;
        return CreatedAtAction(nameof(ListForTarget), new { targetType = req.TargetType, targetId = targetIdForRoute }, new { feedbackId = id });
    }

    // GET /api/feedback/{targetType}/{targetId}
    [HttpGet("{targetType}/{targetId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ListForTarget(string targetType, int targetId, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        var list = await _feedback.ListForTargetAsync(targetType, targetId, ct);
        return Ok(list);
    }

    // GET /api/feedback/{targetType}/{targetId}/summary
    [HttpGet("{targetType}/{targetId:int}/summary")]
    [AllowAnonymous]
    public async Task<IActionResult> Summary(string targetType, int targetId, CancellationToken ct)
    {
        if (!AllowedTargets.Contains(targetType)) return BadRequest("Invalid targetType.");
        var dto = await _feedback.SummaryAsync(targetType, targetId, ct);
        return Ok(new { dto.TargetType, dto.TargetId, count = dto.Count, average = dto.Average });
    }
}
