namespace Relyf.Repository.Dapper.Models;

/// <summary>
/// Extended user profile with follower/following counts and relationship status.
/// </summary>
public sealed class UserProfileDto
{
    public int UserId { get; init; }
    public string Email { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? UserName { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CountryCode { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    
    // Counts
    public int FollowerCount { get; init; }
    public int FollowingCount { get; init; }
    public int ProjectCount { get; init; }
    public int IdeaCount { get; init; }
    public int SaveCount { get; init; }
    
    // Relationship status (from perspective of requesting user)
    public bool IsFollowing { get; init; }
    public bool IsFollowedBy { get; init; }
}
