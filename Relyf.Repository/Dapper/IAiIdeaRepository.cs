using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IAiIdeaRepository
{
    Task<AiIdeaRecord?> GetByIdAsync(int ideaId, int authUserId);

    Task<(IReadOnlyList<AiIdeaRecord> Rows, int Total)> ListByUserAsync(
        int authUserId, int skip, int take, string? orderBy, string? direction, string? search);

    Task<(IReadOnlyList<AiIdeaRecord> Rows, int Total)> ListByItemAsync(
        int authUserId, int itemId, int skip, int take, string? orderBy, string? direction, string? search);

    Task<int> CreateAsync(int authUserId, int itemId, string title, string ideaText, int? coherePromptId);
    Task<int> UpdateAsync(int ideaId, int authUserId, string title, string ideaText);
    Task<int> SoftDeleteAsync(int ideaId, int authUserId);
}
