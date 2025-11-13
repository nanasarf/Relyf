namespace Relyf.Repository.Dapper.Models;

/// <summary>
/// Represents a follow relationship between two users.
/// </summary>
public sealed class FollowRecord
{
    public int FollowId { get; init; }
    public int FollowerId { get; init; }
    public int FollowingId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
