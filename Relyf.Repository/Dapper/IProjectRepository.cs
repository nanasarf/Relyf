using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IProjectRepository
{
    Task<int> CreateAsync(int userId, int? ideaId, string title, string? description, CancellationToken ct = default);
    Task<ProjectRecord?> GetAsync(int projectId, int authUserId, CancellationToken ct = default);
    Task<int> UpdateStatusAsync(int projectId, int authUserId, string status, CancellationToken ct = default);
}
