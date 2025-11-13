using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly IFollowRepository _follows;

    public UsersController(IUserRepository users, IFollowRepository follows)
    {
        _users = users;
        _follows = follows;
    }

    // DTO — keeps the API surface minimal and stable
    public sealed record CreateUserDto(
        [Required, EmailAddress, StringLength(320)] string Email,
        [Required, StringLength(20)] string UserName,
        [Required, StringLength(120)] string DisplayName,
        [StringLength(10)] string? CountryCode
    );

    public sealed record UpdateUserProfileDto(
        [StringLength(20, MinimumLength = 3)] string? UserName,
        [StringLength(120)] string? DisplayName,
        [StringLength(500)] string? Bio,
        string? AvatarUrl
    );

    public sealed record CheckUsernameResponse(bool Available, string? Message);

    /// <summary>Check if a username is available.</summary>
    [HttpGet("check-username/{userName}")]
    public async Task<IActionResult> CheckUsername(string userName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return Ok(new CheckUsernameResponse(false, "Username cannot be empty"));
        }

        if (userName.Length < 3 || userName.Length > 20)
        {
            return Ok(new CheckUsernameResponse(false, "Username must be between 3 and 20 characters"));
        }

        if (!Regex.IsMatch(userName, @"^[a-zA-Z0-9_]+$"))
        {
            return Ok(new CheckUsernameResponse(false, "Username can only contain letters, numbers, and underscores"));
        }

        var exists = await _users.UserNameExistsAsync(userName);
        return Ok(new CheckUsernameResponse(
            !exists,
            exists ? "Username is already taken" : null
        ));
    }

    /// <summary>Create a user.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // Validate username format
        if (!Regex.IsMatch(dto.UserName, @"^[a-zA-Z0-9_]+$"))
        {
            return BadRequest("Username can only contain letters, numbers, and underscores.");
        }

        // Check if username already exists
        if (await _users.UserNameExistsAsync(dto.UserName))
        {
            return Conflict("Username is already taken.");
        }

        // Create and return the new id
        var newId = await _users.CreateAsync(dto.Email.Trim(), dto.UserName.Trim(), dto.DisplayName.Trim(), dto.CountryCode?.Trim());
        // Optionally re-fetch for return payload
        var created = await _users.GetByIdAsync(newId);
        return CreatedAtAction(nameof(GetById), new { id = newId }, (object?)created ?? new { userId = newId });
    }

    /// <summary>Get a user profile by id with counts and relationship status.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var requestingUserId = GetCurrentUserId();
        var profile = await _users.GetProfileAsync(id, requestingUserId);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Update user profile.</summary>
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserProfileDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) 
            return ValidationProblem(ModelState);

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
            return Unauthorized();

        // Ensure user can only update their own profile
        if (currentUserId.Value != id)
            return StatusCode(403, new { error = "Cannot update another user's profile" });

        // Validate username format if provided
        if (dto.UserName != null)
        {
            var trimmedUserName = dto.UserName.Trim();
            
            if (!Regex.IsMatch(trimmedUserName, @"^[a-zA-Z0-9_]+$"))
            {
                return BadRequest(new { error = "Username can only contain letters, numbers, and underscores." });
            }

            // Check if username is already taken by another user
            if (await _users.UserNameExistsAsync(trimmedUserName, id))
            {
                return Conflict(new { error = "Username is already taken." });
            }
        }

        // Perform update
        var rowsAffected = await _users.UpdateProfileAsync(
            id,
            dto.UserName?.Trim(),
            dto.DisplayName?.Trim(),
            dto.Bio?.Trim(),
            dto.AvatarUrl?.Trim()
        );

        if (rowsAffected == 0)
        {
            return NotFound(new { error = "User not found" });
        }

        // Fetch and return updated profile
        var updatedProfile = await _users.GetProfileAsync(id, currentUserId);
        return Ok(updatedProfile);
    }

    /// <summary>Search users by query string.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string query = "",
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        if (take > 100) take = 100; // Limit max results
        if (skip < 0) skip = 0;
        
        var requestingUserId = GetCurrentUserId();
        var result = await _users.SearchAsync(query, skip, take, requestingUserId);
        
        return Ok(result);
    }

    /// <summary>Get followers of a user.</summary>
    [HttpGet("{id:int}/followers")]
    public async Task<IActionResult> GetFollowers(int id, CancellationToken ct)
    {
        var requestingUserId = GetCurrentUserId();
        var followers = await _follows.GetFollowersAsync(id, requestingUserId);
        return Ok(followers);
    }

    /// <summary>Get users that a user is following.</summary>
    [HttpGet("{id:int}/following")]
    public async Task<IActionResult> GetFollowing(int id, CancellationToken ct)
    {
        var requestingUserId = GetCurrentUserId();
        var following = await _follows.GetFollowingAsync(id, requestingUserId);
        return Ok(following);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
