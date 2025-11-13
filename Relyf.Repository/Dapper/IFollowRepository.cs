using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IFollowRepository
{
    /// <summary>
    /// Create a follow relationship.
    /// </summary>
    Task<FollowRecord?> CreateFollowAsync(int followerId, int followingId);
    
    /// <summary>
    /// Remove a follow relationship.
    /// </summary>
    Task<bool> DeleteFollowAsync(int followerId, int followingId);
    
    /// <summary>
    /// Check if a user is following another user.
    /// </summary>
    Task<bool> IsFollowingAsync(int followerId, int followingId);
    
    /// <summary>
    /// Get all followers of a user.
    /// </summary>
    Task<List<UserProfileDto>> GetFollowersAsync(int userId, int? requestingUserId = null);
    
    /// <summary>
    /// Get all users that a user is following.
    /// </summary>
    Task<List<UserProfileDto>> GetFollowingAsync(int userId, int? requestingUserId = null);
    
    /// <summary>
    /// Get follower count for a user.
    /// </summary>
    Task<int> GetFollowerCountAsync(int userId);
    
    /// <summary>
    /// Get following count for a user.
    /// </summary>
    Task<int> GetFollowingCountAsync(int userId);
}
