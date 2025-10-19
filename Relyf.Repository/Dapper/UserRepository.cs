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
                @"SELECT UserId, Email, DisplayName, CountryCode, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.[User]
                  WHERE UserId = @userId;",
                new { userId }));

    public Task<UserRecord?> GetByEmailAsync(string email) =>
        WithConnection(conn =>
            conn.QuerySingleOrDefaultAsync<UserRecord>(
                @"SELECT UserId, Email, DisplayName, CountryCode, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.[User]
                  WHERE Email = @email;",
                new { email }));

    public Task<int> CreateAsync(string email, string displayName, string? countryCode) =>
        WithConnection(conn =>
            conn.ExecuteScalarAsync<int>(
                @"INSERT INTO app.[User] (Email, DisplayName, CountryCode, CreatedAtUtc, IsDeleted)
                  VALUES (@Email, @DisplayName, @CountryCode, SYSUTCDATETIME(), 0);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { Email = email, DisplayName = displayName, CountryCode = countryCode }));

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

    public Task<int> SoftDeleteAsync(int userId) =>
        WithConnection(conn =>
            conn.ExecuteAsync(
                @"UPDATE app.[User]
                  SET IsDeleted = 1,
                      UpdatedAtUtc = SYSUTCDATETIME()
                  WHERE UserId = @UserId;",
                new { UserId = userId }));
}
