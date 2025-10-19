using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class CommentRepository : BaseRepository, ICommentRepository
{
    public CommentRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, string targetType, int targetId, string body, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.Comment (UserId, TargetType, TargetId, Body, CreatedAtUtc)
                  VALUES (@userId, @targetType, @targetId, @body, SYSUTCDATETIME());
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, targetType, targetId, body }, cancellationToken: ct)));

    public Task<IReadOnlyList<CommentRecord>> ListForTargetAsync(string targetType, int targetId, int take, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT TOP (@take) CommentId, UserId, TargetType, TargetId, Body, CreatedAtUtc
FROM app.Comment
WHERE TargetType = @targetType AND TargetId = @targetId
ORDER BY CommentId DESC;";
            var list = (await conn.QueryAsync<CommentRecord>(
                new CommandDefinition(sql, new { targetType, targetId, take }, cancellationToken: ct))).ToList();
            IReadOnlyList<CommentRecord> rows = list;
            return rows;
        });

    public Task<int> DeleteIfOwnerAsync(int commentId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                @"DELETE FROM app.Comment WHERE CommentId = @commentId AND UserId = @authUserId;",
                new { commentId, authUserId }, cancellationToken: ct)));
}
