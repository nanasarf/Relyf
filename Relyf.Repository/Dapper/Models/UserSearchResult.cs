namespace Relyf.Repository.Dapper.Models;

/// <summary>
/// Search result with pagination info.
/// </summary>
public sealed class UserSearchResult
{
    public List<UserProfileDto> Results { get; init; } = new();
    public int Total { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}
