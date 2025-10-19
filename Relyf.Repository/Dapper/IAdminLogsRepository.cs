using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IAdminLogsRepository
{
    Task<IReadOnlyList<ApiLogRecentRow>> GetRecentAsync(
        int sinceId, int? userId, int statusMin, int statusMax, int take, CancellationToken ct = default);

    Task<ApiLogSummaryRow> GetSummaryAsync(int maxId, CancellationToken ct = default);

    Task<IReadOnlyList<ApiLogTopModelRow>> GetTopModelsAsync(int take, CancellationToken ct = default);
}
