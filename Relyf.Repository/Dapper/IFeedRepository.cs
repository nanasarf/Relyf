using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

/// <summary>
/// Repository for retrieving feed items from followed users.
/// </summary>
public interface IFeedRepository
{
    /// <summary>
    /// Get feed items (projects and AI ideas) from users that the authenticated user follows.
    /// Results are ordered by creation date (newest first).
    /// </summary>
    /// <param name="userId">The authenticated user's ID</param>
    /// <param name="skip">Number of items to skip (for pagination)</param>
    /// <param name="take">Number of items to return</param>
    /// <returns>Feed result with items and pagination info</returns>
    Task<FeedResult> GetFollowingFeedAsync(int userId, int skip = 0, int take = 20);
}
