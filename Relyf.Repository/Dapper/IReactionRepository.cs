namespace Relyf.Repository.Dapper;

public interface IReactionRepository
{
    Task<bool> PutAsync(int userId, string targetType, int targetId, string kind, CancellationToken ct = default);
    Task<int> DeleteAsync(int userId, string targetType, int targetId, string kind, CancellationToken ct = default);
    Task<int> CountAsync(string targetType, int targetId, string kind, CancellationToken ct = default);
}
