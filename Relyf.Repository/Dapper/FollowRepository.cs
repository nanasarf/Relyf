using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class FollowRepository : BaseRepository, IFollowRepository
{
    public FollowRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<FollowRecord?> CreateFollowAsync(int followerId, int followingId) =>
        WithConnection(async conn =>
        {
            try
            {
                var id = await conn.ExecuteScalarAsync<int>(
                    @"INSERT INTO app.[Follow] (FollowerId, FollowingId, CreatedAtUtc)
                      VALUES (@FollowerId, @FollowingId, SYSUTCDATETIME());
                      SELECT CAST(SCOPE_IDENTITY() AS int);",
                    new { FollowerId = followerId, FollowingId = followingId });

                return await conn.QuerySingleOrDefaultAsync<FollowRecord>(
                    @"SELECT FollowId, FollowerId, FollowingId, CreatedAtUtc
                      FROM app.[Follow]
                      WHERE FollowId = @id;",
                    new { id });
            }
            catch
            {
                // Likely duplicate or constraint violation
                return null;
            }
        });

    public Task<bool> DeleteFollowAsync(int followerId, int followingId) =>
        WithConnection(async conn =>
        {
            var affected = await conn.ExecuteAsync(
                @"DELETE FROM app.[Follow]
                  WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId;",
                new { FollowerId = followerId, FollowingId = followingId });
            
            return affected > 0;
        });

    public Task<bool> IsFollowingAsync(int followerId, int followingId) =>
        WithConnection(async conn =>
        {
            var exists = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(1)
                  FROM app.[Follow]
                  WHERE FollowerId = @FollowerId AND FollowingId = @FollowingId;",
                new { FollowerId = followerId, FollowingId = followingId });
            
            return exists > 0;
        });

    public Task<List<UserProfileDto>> GetFollowersAsync(int userId, int? requestingUserId = null) =>
        WithConnection(async conn =>
        {
            var sql = @"
                SELECT 
                    u.UserId,
                    u.Email,
                    u.DisplayName,
                    NULL as UserName,
                    NULL as Bio,
                    NULL as AvatarUrl,
                    u.CountryCode,
                    u.CreatedAtUtc,
                    u.UpdatedAtUtc,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowingId = u.UserId) as FollowerCount,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowerId = u.UserId) as FollowingCount,
                    (SELECT COUNT(*) FROM app.[Project] WHERE UserId = u.UserId AND IsDeleted = 0) as ProjectCount,
                    (SELECT COUNT(*) FROM app.[AiIdea] WHERE UserId = u.UserId AND IsDeleted = 0) as IdeaCount,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = @RequestingUserId AND FollowingId = u.UserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowing,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = u.UserId AND FollowingId = @RequestingUserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowedBy
                FROM app.[Follow] f
                INNER JOIN app.[User] u ON f.FollowerId = u.UserId
                WHERE f.FollowingId = @UserId
                  AND u.IsDeleted = 0
                ORDER BY f.CreatedAtUtc DESC;";

            var results = await conn.QueryAsync<UserProfileDto>(sql, 
                new { UserId = userId, RequestingUserId = requestingUserId });
            
            return results.ToList();
        });

    public Task<List<UserProfileDto>> GetFollowingAsync(int userId, int? requestingUserId = null) =>
        WithConnection(async conn =>
        {
            var sql = @"
                SELECT 
                    u.UserId,
                    u.Email,
                    u.DisplayName,
                    NULL as UserName,
                    NULL as Bio,
                    NULL as AvatarUrl,
                    u.CountryCode,
                    u.CreatedAtUtc,
                    u.UpdatedAtUtc,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowingId = u.UserId) as FollowerCount,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowerId = u.UserId) as FollowingCount,
                    (SELECT COUNT(*) FROM app.[Project] WHERE UserId = u.UserId AND IsDeleted = 0) as ProjectCount,
                    (SELECT COUNT(*) FROM app.[AiIdea] WHERE UserId = u.UserId AND IsDeleted = 0) as IdeaCount,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = @RequestingUserId AND FollowingId = u.UserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowing,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = u.UserId AND FollowingId = @RequestingUserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowedBy
                FROM app.[Follow] f
                INNER JOIN app.[User] u ON f.FollowingId = u.UserId
                WHERE f.FollowerId = @UserId
                  AND u.IsDeleted = 0
                ORDER BY f.CreatedAtUtc DESC;";

            var results = await conn.QueryAsync<UserProfileDto>(sql, 
                new { UserId = userId, RequestingUserId = requestingUserId });
            
            return results.ToList();
        });

    public Task<int> GetFollowerCountAsync(int userId) =>
        WithConnection(conn =>
            conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM app.[Follow]
                  WHERE FollowingId = @userId;",
                new { userId }));

    public Task<int> GetFollowingCountAsync(int userId) =>
        WithConnection(conn =>
            conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*)
                  FROM app.[Follow]
                  WHERE FollowerId = @userId;",
                new { userId }));
}
