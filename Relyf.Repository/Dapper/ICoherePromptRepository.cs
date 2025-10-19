using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface ICoherePromptRepository
{
    Task<int> CreateAsync(int userId, int? itemId, string? model, decimal? temperature, decimal? topP, string promptText, CancellationToken ct = default);
    Task<CoherePromptRecord?> GetAsync(int coherePromptId, int userId, CancellationToken ct = default);
}
