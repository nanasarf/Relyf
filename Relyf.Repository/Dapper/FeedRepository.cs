using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class FeedRepository : BaseRepository, IFeedRepository
{
    public FeedRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<FeedResult> GetFollowingFeedAsync(int userId, int skip = 0, int take = 20) =>
        WithConnection(async conn =>
        {
            // Get total count of feed items from followed users
            var totalSql = @"
                SELECT COUNT(*) 
                FROM (
                    -- Count projects from followed users
                    SELECT p.ProjectId
                    FROM app.[Project] p
                    INNER JOIN app.[Follow] f ON p.UserId = f.FollowingId
                    WHERE f.FollowerId = @UserId 
                      AND p.IsDeleted = 0
                    
                    UNION ALL
                    
                    -- Count AI ideas from followed users
                    SELECT ai.IdeaId
                    FROM app.[AiIdea] ai
                    INNER JOIN app.[Follow] f ON ai.UserId = f.FollowingId
                    WHERE f.FollowerId = @UserId 
                      AND ai.IsDeleted = 0
                ) AS AllItems;";
            
            var total = await conn.ExecuteScalarAsync<int>(totalSql, new { UserId = userId });
            
            // Get paginated feed items with user info
            // Note: Engagement metrics (reactions, comments, saves) will be added when those tables are implemented
            var itemsSql = @"
                SELECT 
                    'project' AS ItemType,
                    p.ProjectId AS ItemId,
                    p.UserId,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    p.Title,
                    p.Description,
                    NULL AS IdeaText,
                    p.CreatedAtUtc,
                    p.UpdatedAtUtc,
                    p.Status,
                    p.IdeaId,
                    p.AiIdeaId,
                    (SELECT TOP 1 i.Url 
                     FROM app.Image i 
                     WHERE i.OwnerType = 'Project' 
                       AND i.OwnerId = p.ProjectId 
                     ORDER BY i.CreatedAtUtc ASC) AS ImageUrl,
                    0 AS ReactionCount,
                    0 AS CommentCount,
                    0 AS SaveCount,
                    CAST(0 AS BIT) AS HasUserReacted,
                    CAST(0 AS BIT) AS HasUserSaved
                FROM app.[Project] p
                INNER JOIN app.[Follow] f ON p.UserId = f.FollowingId
                INNER JOIN app.[User] u ON p.UserId = u.UserId
                WHERE f.FollowerId = @UserId 
                  AND p.IsDeleted = 0
                  AND u.IsDeleted = 0
                
                UNION ALL
                
                SELECT 
                    'idea' AS ItemType,
                    ai.IdeaId AS ItemId,
                    ai.UserId,
                    u.UserName,
                    u.DisplayName,
                    u.AvatarUrl,
                    ai.Title,
                    NULL AS Description,
                    ai.IdeaText,
                    ai.CreatedAtUtc,
                    ai.UpdatedAtUtc,
                    NULL AS Status,
                    NULL AS IdeaId,
                    NULL AS AiIdeaId,
                    NULL AS ImageUrl,
                    0 AS ReactionCount,
                    0 AS CommentCount,
                    0 AS SaveCount,
                    CAST(0 AS BIT) AS HasUserReacted,
                    CAST(0 AS BIT) AS HasUserSaved
                FROM app.[AiIdea] ai
                INNER JOIN app.[Follow] f ON ai.UserId = f.FollowingId
                INNER JOIN app.[User] u ON ai.UserId = u.UserId
                WHERE f.FollowerId = @UserId 
                  AND ai.IsDeleted = 0
                  AND u.IsDeleted = 0
                
                ORDER BY CreatedAtUtc DESC
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY;";
            
            var items = await conn.QueryAsync<FeedItemDto>(itemsSql, 
                new { UserId = userId, Skip = skip, Take = take });
            
            return new FeedResult
            {
                Items = items.ToList(),
                Total = total,
                Skip = skip,
                Take = take
            };
        });
}
