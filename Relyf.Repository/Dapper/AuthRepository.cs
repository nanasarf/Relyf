using System.Data;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class AuthRepository : BaseRepository, IAuthRepository
{
    public AuthRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.[User] WHERE Email = @email;";
            var n = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { email }, cancellationToken: ct));
            return n > 0;
        });

    public Task<bool> UserNameExistsAsync(string userName, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.[User] WHERE UserName = @userName;";
            var n = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { userName }, cancellationToken: ct));
            return n > 0;
        });

    public Task<UserAuthRecord?> GetUserByEmailAsync(string email, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<UserAuthRecord>(
            new CommandDefinition(
                @"SELECT UserId, Email, DisplayName, CountryCode
                  FROM app.[User]
                  WHERE Email = @email AND IsDeleted = 0;",
                new { email }, cancellationToken: ct)));

    public Task<UserCredentialRecord?> GetCredentialAsync(int userId, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<UserCredentialRecord>(
            new CommandDefinition(
                @"SELECT UserId, PasswordHash, PasswordSalt
                  FROM app.UserCredential
                  WHERE UserId = @userId;",
                new { userId }, cancellationToken: ct)));

    public Task<UserAuthRecord> CreateUserWithCredentialAsync(
        string email, string userName, string displayName, string? countryCode,
        byte[] passwordHash, byte[] passwordSalt, CancellationToken ct = default)
        => WithConnection(async conn =>
        {
            using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

            // Insert user
            const string insertUser = @"
INSERT INTO app.[User] (Email, UserName, DisplayName, CountryCode, IsDeleted)
VALUES (@email, @userName, @displayName, @countryCode, 0);
SELECT CAST(SCOPE_IDENTITY() AS int);";

            var userId = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(insertUser, new { email, userName, displayName, countryCode }, tx, cancellationToken: ct));

            // Insert credential
            const string insertCred = @"
INSERT INTO app.UserCredential (UserId, PasswordHash, PasswordSalt)
VALUES (@userId, @passwordHash, @passwordSalt);";

            await conn.ExecuteAsync(new CommandDefinition(
                insertCred, new { userId, passwordHash, passwordSalt }, tx, cancellationToken: ct));

            tx.Commit();

            return new UserAuthRecord
            {
                UserId = userId,
                Email = email,
                UserName = userName,
                DisplayName = displayName,
                CountryCode = countryCode
            };
        });
}
