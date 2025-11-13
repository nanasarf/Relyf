using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FollowController : ControllerBase
{
    private readonly IFollowRepository _follows;

    public FollowController(IFollowRepository follows) => _follows = follows;

    /// <summary>
    /// Follow a user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Follow([FromBody] FollowRequest request, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        // Prevent self-following
        if (currentUserId == request.FollowingId)
            return BadRequest(new { error = "Cannot follow yourself" });

        var result = await _follows.CreateFollowAsync(currentUserId.Value, request.FollowingId);
        
        if (result == null)
            return Conflict(new { error = "Already following this user or invalid user ID" });

        return CreatedAtAction(nameof(CheckFollowStatus), new { followingId = request.FollowingId }, result);
    }

    /// <summary>
    /// Unfollow a user.
    /// </summary>
    [HttpDelete("{followingId:int}")]
    public async Task<IActionResult> Unfollow(int followingId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var success = await _follows.DeleteFollowAsync(currentUserId.Value, followingId);
        
        if (!success)
            return NotFound(new { error = "Follow relationship not found" });

        return NoContent();
    }

    /// <summary>
    /// Check if the current user is following a specific user.
    /// </summary>
    [HttpGet("check/{followingId:int}")]
    public async Task<IActionResult> CheckFollowStatus(int followingId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        var isFollowing = await _follows.IsFollowingAsync(currentUserId.Value, followingId);
        
        return Ok(new { isFollowing });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public sealed record FollowRequest(int FollowingId);
}
