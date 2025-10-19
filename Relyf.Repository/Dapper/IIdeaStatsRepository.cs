using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IIdeaStatsRepository
{
    Task<IdeaStatsDto?> GetIdeaStatsAsync(int ideaId, CancellationToken ct = default);
    Task<IReadOnlyList<TopIdeaDto>> GetTopIdeasAsync(int take, CancellationToken ct = default);
}
