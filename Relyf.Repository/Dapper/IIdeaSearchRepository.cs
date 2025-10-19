using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IIdeaSearchRepository
{
    Task<(IReadOnlyList<IdeaSearchRow> Rows, int Total)> SearchAsync(
        string? q, string? tag, int? userId, int skip, int take, CancellationToken ct = default);
}
