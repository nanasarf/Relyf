using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<UserRecord?> GetByIdAsync(int userId) =>
        WithConnection(conn =>
            conn.QuerySingleOrDefaultAsync<UserRecord>(
                @"SELECT UserId, Email, UserName, DisplayName, Bio, AvatarUrl, CountryCode, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.[User]
                  WHERE UserId = @userId;",
                new { userId }));

    public Task<UserRecord?> GetByEmailAsync(string email) =>
        WithConnection(conn =>
            conn.QuerySingleOrDefaultAsync<UserRecord>(
                @"SELECT UserId, Email, UserName, DisplayName, Bio, AvatarUrl, CountryCode, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.[User]
                  WHERE Email = @email;",
                new { email }));

    public Task<UserRecord?> GetByUserNameAsync(string userName) =>
        WithConnection(conn =>
            conn.QuerySingleOrDefaultAsync<UserRecord>(
                @"SELECT UserId, Email, UserName, DisplayName, Bio, AvatarUrl, CountryCode, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.[User]
                  WHERE UserName = @userName;",
                new { userName }));

    public Task<bool> UserNameExistsAsync(string userName) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.[User] WHERE UserName = @userName;";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { userName });
            return count > 0;
        });

    public Task<bool> UserNameExistsAsync(string userName, int excludeUserId) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.[User] WHERE UserName = @userName AND UserId != @excludeUserId;";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { userName, excludeUserId });
            return count > 0;
        });

    public Task<int> CreateAsync(string email, string userName, string displayName, string? countryCode) =>
        WithConnection(conn =>
            conn.ExecuteScalarAsync<int>(
                @"INSERT INTO app.[User] (Email, UserName, DisplayName, CountryCode, CreatedAtUtc, IsDeleted)
                  VALUES (@Email, @UserName, @DisplayName, @CountryCode, SYSUTCDATETIME(), 0);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { Email = email, UserName = userName, DisplayName = displayName, CountryCode = countryCode }));

    public Task<int> UpdateAsync(int userId, string email, string displayName, string? countryCode, bool isDeleted) =>
        WithConnection(conn =>
            conn.ExecuteAsync(
                @"UPDATE app.[User]
                  SET Email = @Email,
                      DisplayName = @DisplayName,
                      CountryCode = @CountryCode,
                      UpdatedAtUtc = SYSUTCDATETIME(),
                      IsDeleted = @IsDeleted
                  WHERE UserId = @UserId;",
                new { UserId = userId, Email = email, DisplayName = displayName, CountryCode = countryCode, IsDeleted = isDeleted }));

    public Task<int> UpdateProfileAsync(int userId, string? userName, string? displayName, string? bio, string? avatarUrl) =>
        WithConnection(async conn =>
        {
            var setClauses = new List<string> { "UpdatedAtUtc = SYSUTCDATETIME()" };
            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);

            if (userName != null)
            {
                setClauses.Add("UserName = @UserName");
                parameters.Add("UserName", userName);
            }
            if (displayName != null)
            {
                setClauses.Add("DisplayName = @DisplayName");
                parameters.Add("DisplayName", displayName);
            }
            if (bio != null)
            {
                setClauses.Add("Bio = @Bio");
                parameters.Add("Bio", bio);
            }
            if (avatarUrl != null)
            {
                setClauses.Add("AvatarUrl = @AvatarUrl");
                parameters.Add("AvatarUrl", avatarUrl);
            }

            if (setClauses.Count == 1) // Only UpdatedAtUtc, nothing to update
                return 0;

            var sql = $@"UPDATE app.[User] 
                         SET {string.Join(", ", setClauses)}
                         WHERE UserId = @UserId;";

            return await conn.ExecuteAsync(sql, parameters);
        });

    public Task<int> SoftDeleteAsync(int userId) =>
        WithConnection(conn =>
            conn.ExecuteAsync(
                @"UPDATE app.[User]
                  SET IsDeleted = 1,
                      UpdatedAtUtc = SYSUTCDATETIME()
                  WHERE UserId = @UserId;",
                new { UserId = userId }));

    public Task<UserSearchResult> SearchAsync(string query, int skip, int take, int? requestingUserId = null) =>
        WithConnection(async conn =>
        {
            var searchTerm = $"%{query}%";
            
            // Get total count
            var totalSql = @"
                SELECT COUNT(*)
                FROM app.[User]
                WHERE IsDeleted = 0
                  AND (DisplayName LIKE @SearchTerm 
                       OR UserName LIKE @SearchTerm
                       OR Email LIKE @SearchTerm);";
            
            var total = await conn.ExecuteScalarAsync<int>(totalSql, new { SearchTerm = searchTerm });
            
            // Get paginated results with profile data
            var resultsSql = @"
                SELECT 
                    u.UserId,
                    u.Email,
                    u.UserName,
                    u.DisplayName,
                    u.Bio,
                    u.AvatarUrl,
                    u.CountryCode,
                    u.CreatedAtUtc,
                    u.UpdatedAtUtc,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowingId = u.UserId) as FollowerCount,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowerId = u.UserId) as FollowingCount,
                    (SELECT COUNT(*) FROM app.[Project] WHERE UserId = u.UserId AND IsDeleted = 0) as ProjectCount,
                    (SELECT COUNT(*) FROM app.[AiIdea] WHERE UserId = u.UserId AND IsDeleted = 0) as IdeaCount,
                    (SELECT COUNT(*) FROM app.[SavedIdea] WHERE UserId = u.UserId) as SaveCount,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = @RequestingUserId AND FollowingId = u.UserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowing,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = u.UserId AND FollowingId = @RequestingUserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowedBy
                FROM app.[User] u
                WHERE u.IsDeleted = 0
                  AND (u.DisplayName LIKE @SearchTerm 
                       OR u.UserName LIKE @SearchTerm
                       OR u.Email LIKE @SearchTerm)
                ORDER BY u.DisplayName
                OFFSET @Skip ROWS
                FETCH NEXT @Take ROWS ONLY;";
            
            var results = await conn.QueryAsync<UserProfileDto>(resultsSql, 
                new { SearchTerm = searchTerm, Skip = skip, Take = take, RequestingUserId = requestingUserId });
            
            return new UserSearchResult
            {
                Results = results.ToList(),
                Total = total,
                Skip = skip,
                Take = take
            };
        });

    public Task<UserProfileDto?> GetProfileAsync(int userId, int? requestingUserId = null) =>
        WithConnection(conn =>
            conn.QuerySingleOrDefaultAsync<UserProfileDto>(
                @"SELECT 
                    u.UserId,
                    u.Email,
                    u.UserName,
                    u.DisplayName,
                    u.Bio,
                    u.AvatarUrl,
                    u.CountryCode,
                    u.CreatedAtUtc,
                    u.UpdatedAtUtc,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowingId = u.UserId) as FollowerCount,
                    (SELECT COUNT(*) FROM app.[Follow] WHERE FollowerId = u.UserId) as FollowingCount,
                    (SELECT COUNT(*) FROM app.[Project] WHERE UserId = u.UserId AND IsDeleted = 0) as ProjectCount,
                    (SELECT COUNT(*) FROM app.[AiIdea] WHERE UserId = u.UserId AND IsDeleted = 0) as IdeaCount,
                    (SELECT COUNT(*) FROM app.[SavedIdea] WHERE UserId = u.UserId) as SaveCount,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = @RequestingUserId AND FollowingId = u.UserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowing,
                    CASE WHEN @RequestingUserId IS NOT NULL AND EXISTS (
                        SELECT 1 FROM app.[Follow] 
                        WHERE FollowerId = u.UserId AND FollowingId = @RequestingUserId
                    ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END as IsFollowedBy
                  FROM app.[User] u
                  WHERE u.UserId = @UserId;",
                new { UserId = userId, RequestingUserId = requestingUserId }));
}
