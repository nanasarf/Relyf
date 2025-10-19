using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class FeedbackRepository : BaseRepository, IFeedbackRepository
{
    public FeedbackRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, string targetType, int? targetId, byte? rating, string? notes, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.Feedback (UserId, TargetType, TargetId, Rating, Notes, CreatedAtUtc)
                  VALUES (@userId, @targetType, @targetId, @rating, @notes, SYSUTCDATETIME());
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, targetType, targetId, rating, notes },
                cancellationToken: ct)));

    public Task<IReadOnlyList<FeedbackRecord>> ListForTargetAsync(string targetType, int targetId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT FeedbackId, UserId, TargetType, TargetId, Rating, Notes, CreatedAtUtc
FROM app.Feedback
WHERE TargetType = @targetType AND TargetId = @targetId
ORDER BY FeedbackId DESC;";
            var rows = (await conn.QueryAsync<FeedbackRecord>(
                new CommandDefinition(sql, new { targetType, targetId }, cancellationToken: ct))).ToList();
            IReadOnlyList<FeedbackRecord> list = rows;
            return list;
        });

    public Task<FeedbackSummary> SummaryAsync(string targetType, int targetId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT
  @targetType AS TargetType,
  @targetId   AS TargetId,
  COUNT(1)    AS Count,
  CASE WHEN COUNT(1)=0 THEN NULL ELSE ROUND(AVG(CAST(Rating AS float)), 2) END AS Average
FROM app.Feedback
WHERE TargetType = @targetType AND TargetId = @targetId;";
            return await conn.QuerySingleAsync<FeedbackSummary>(
                new CommandDefinition(sql, new { targetType, targetId }, cancellationToken: ct));
        });
}
