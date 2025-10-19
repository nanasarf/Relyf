using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IFeedbackRepository
{
    Task<int> CreateAsync(int userId, string targetType, int? targetId, byte? rating, string? notes, CancellationToken ct = default);
    Task<IReadOnlyList<FeedbackRecord>> ListForTargetAsync(string targetType, int targetId, CancellationToken ct = default);
    Task<FeedbackSummary> SummaryAsync(string targetType, int targetId, CancellationToken ct = default);
}
