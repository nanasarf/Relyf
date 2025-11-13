using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class AdminLogsRepository : BaseRepository, IAdminLogsRepository
{
    public AdminLogsRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<IReadOnlyList<ApiLogRecentRow>> GetRecentAsync(
        int sinceId, int? userId, int statusMin, int statusMax, int take, CancellationToken ct = default)
        => WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 500);
            var where = "l.ApiRequestLogId > @sinceId AND l.StatusCode BETWEEN @statusMin AND @statusMax";
            var dp = new DynamicParameters(new { sinceId, statusMin, statusMax, take });
            if (userId is not null)
            {
                where += " AND l.UserId = @userId";
                dp.Add("userId", userId.Value);
            }

            var sql = $@"
SELECT TOP (@take)
  l.ApiRequestLogId, l.UserId, l.Provider, l.Endpoint, l.Model,
  l.TokensIn, l.TokensOut, l.StatusCode, l.DurationMs
FROM app.ApiRequestLog l
WHERE {where}
ORDER BY l.ApiRequestLogId DESC;";

            var rows = (await conn.QueryAsync<ApiLogRecentRow>(
                new CommandDefinition(sql, dp, cancellationToken: ct))).ToList();
            IReadOnlyList<ApiLogRecentRow> list = rows;
            return list;
        });

    public Task<ApiLogSummaryRow> GetSummaryAsync(int maxId, CancellationToken ct = default)
        => WithConnection(async conn =>
        {
            var whereClause = maxId > 0 ? "WHERE ApiRequestLogId <= @maxId" : "";
            var dp = new DynamicParameters(new { maxId });

            // totals
            var total = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT COUNT(1) FROM app.ApiRequestLog {whereClause};", dp, cancellationToken: ct));

            var errors = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    maxId > 0
                        ? "SELECT COUNT(1) FROM app.ApiRequestLog WHERE ApiRequestLogId <= @maxId AND StatusCode >= 400;"
                        : "SELECT COUNT(1) FROM app.ApiRequestLog WHERE StatusCode >= 400;",
                    dp,
                    cancellationToken: ct));

            var avgLatency = await conn.ExecuteScalarAsync<double?>(
                new CommandDefinition($"SELECT AVG(CAST(ISNULL(DurationMs,0) AS float)) FROM app.ApiRequestLog {whereClause};", dp, cancellationToken: ct));

            var tokensIn = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT SUM(ISNULL(TokensIn,0)) FROM app.ApiRequestLog {whereClause};", dp, cancellationToken: ct));

            var tokensOut = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT SUM(ISNULL(TokensOut,0)) FROM app.ApiRequestLog {whereClause};", dp, cancellationToken: ct));

            return new ApiLogSummaryRow
            {
                Total = total,
                Errors = errors,
                ErrorRate = total == 0 ? 0 : Math.Round(errors * 100.0 / total, 2),
                AvgLatencyMs = avgLatency is null ? null : Math.Round(avgLatency.Value, 2),
                TokensIn = tokensIn,
                TokensOut = tokensOut
            };
        });

    public Task<IReadOnlyList<ApiLogTopModelRow>> GetTopModelsAsync(int take, CancellationToken ct = default)
        => WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 20);
            const string sql = @"
SELECT TOP (@take)
  ISNULL(Model,'(unknown)') AS Model,
  COUNT(*)                  AS Calls,
  ROUND(AVG(CAST(ISNULL(DurationMs,0) AS float)), 2) AS AvgLatencyMs,
  ROUND(100.0 * SUM(CASE WHEN StatusCode >= 400 THEN 1 ELSE 0 END) / NULLIF(COUNT(*),0), 2) AS ErrorRate
FROM app.ApiRequestLog
GROUP BY ISNULL(Model,'(unknown)')
ORDER BY Calls DESC;";
            var rows = (await conn.QueryAsync<ApiLogTopModelRow>(
                new CommandDefinition(sql, new { take }, cancellationToken: ct))).ToList();
            IReadOnlyList<ApiLogTopModelRow> list = rows;
            return list;
        });
}
