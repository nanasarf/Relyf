using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IProjectRepository
{
    Task<int> CreateAsync(int userId, int? ideaId, int? aiIdeaId, string title, string? description, CancellationToken ct = default);
    Task<ProjectRecord?> GetAsync(int projectId, int authUserId, CancellationToken ct = default);
    Task<int> UpdateStatusAsync(int projectId, int authUserId, string status, CancellationToken ct = default);
    Task<IEnumerable<ProjectRecord>> ListAsync(int authUserId, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(int authUserId, CancellationToken ct = default);
    Task<IEnumerable<ProjectRecord>> GetUserProjectsAsync(int userId, int skip, int take, CancellationToken ct = default);
    Task<int> CountUserProjectsAsync(int userId, CancellationToken ct = default);
    Task<int> SoftDeleteAsync(int projectId, int authUserId, CancellationToken ct = default); // NEW
}
