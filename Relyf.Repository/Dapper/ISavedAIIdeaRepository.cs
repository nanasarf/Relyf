using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface ISavedAIIdeaRepository
{
    Task<SavedAIIdeaRecord?> GetByIdAsync(int aiIdeaId, int authUserId);
    Task<(IReadOnlyList<SavedAIIdeaRecord> Rows, int Total)> ListByUserAsync(int authUserId, int skip, int take);
    Task<int> CreateAsync(int userId, string title, string? tools, string? steps, string? safety);
    Task<int> UpdateAsync(int aiIdeaId, int authUserId, string title, string? tools, string? steps, string? safety);
    Task<int> SoftDeleteAsync(int aiIdeaId, int authUserId);
}
