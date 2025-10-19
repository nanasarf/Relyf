using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface ISaveRepository
{
    // Returns true if a new row was inserted, false if it already existed
    Task<bool> PutAsync(int userId, int ideaId, CancellationToken ct = default);

    // Returns number of rows deleted (0 or 1)
    Task<int> DeleteAsync(int userId, int ideaId, CancellationToken ct = default);

    Task<IReadOnlyList<SavedIdeaView>> ListForUserAsync(int userId, CancellationToken ct = default);
}
