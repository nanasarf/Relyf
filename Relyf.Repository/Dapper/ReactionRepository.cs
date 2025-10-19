using Dapper;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ReactionRepository : BaseRepository, IReactionRepository
{
    public ReactionRepository(IDbConnectionFactory factory) : base(factory) { }

    // Returns true if newly inserted, false if already existed
    public Task<bool> PutAsync(int userId, string targetType, int targetId, string kind, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            // Unique constraint recommended: UX_Reaction_User_Target_Kind (UserId, TargetType, TargetId, Kind)
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM app.Reaction WHERE UserId=@userId AND TargetType=@targetType AND TargetId=@targetId AND Kind=@kind)
BEGIN
    INSERT INTO app.Reaction (UserId, TargetType, TargetId, Kind, CreatedAtUtc)
    VALUES (@userId, @targetType, @targetId, @kind, SYSUTCDATETIME());
    SELECT 1;
END
ELSE SELECT 0;";
            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { userId, targetType, targetId, kind }, cancellationToken: ct)) == 1;
        });

    public Task<int> DeleteAsync(int userId, string targetType, int targetId, string kind, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM app.Reaction WHERE UserId=@userId AND TargetType=@targetType AND TargetId=@targetId AND Kind=@kind;",
                new { userId, targetType, targetId, kind }, cancellationToken: ct)));

    public Task<int> CountAsync(string targetType, int targetId, string kind, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "SELECT COUNT(1) FROM app.Reaction WHERE TargetType=@targetType AND TargetId=@targetId AND Kind=@kind;",
                new { targetType, targetId, kind }, cancellationToken: ct)));
}
