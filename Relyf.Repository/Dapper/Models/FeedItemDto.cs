namespace Relyf.Repository.Dapper.Models;

/// <summary>
/// Represents a feed item that can be either a project or an AI idea from followed users.
/// </summary>
public sealed class FeedItemDto
{
    public string ItemType { get; init; } = ""; // "project" or "idea"
    public int ItemId { get; init; }
    public int UserId { get; init; }
    public string UserName { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string? IdeaText { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    
    // Project-specific fields
    public string? Status { get; init; }
    public int? IdeaId { get; init; }
    public int? AiIdeaId { get; init; }
    public string? ImageUrl { get; init; }  // First image URL for projects
    
    // Engagement metrics
    public int ReactionCount { get; init; }
    public int CommentCount { get; init; }
    public int SaveCount { get; init; }
    
    // User interaction status
    public bool HasUserReacted { get; init; }
    public bool HasUserSaved { get; init; }
}

/// <summary>
/// Feed result with pagination info.
/// </summary>
public sealed class FeedResult
{
    public List<FeedItemDto> Items { get; init; } = new();
    public int Total { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}
