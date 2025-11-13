using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class FeedController : ControllerBase
{
    private readonly IFeedRepository _feed;

    public FeedController(IFeedRepository feed)
    {
        _feed = feed;
    }

    /// <summary>
    /// Get feed of projects and ideas from users you follow.
    /// Returns a chronological feed of content from followed users (newest first).
    /// </summary>
    /// <param name="skip">Number of items to skip (for pagination)</param>
    /// <param name="take">Number of items to return (max 100)</param>
    /// <returns>Feed items with pagination info</returns>
    [HttpGet]
    public async Task<IActionResult> GetFollowingFeed(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        // Validate pagination parameters
        if (skip < 0) skip = 0;
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token" });
        }

        // Get feed items
        var result = await _feed.GetFollowingFeedAsync(userId, skip, take);

        return Ok(result);
    }
}
