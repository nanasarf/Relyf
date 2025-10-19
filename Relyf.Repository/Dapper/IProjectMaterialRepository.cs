using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IProjectMaterialRepository
{
    Task<int> UpsertAsync(int projectId, int materialId, string? quantityText, int authUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectMaterialView>> ListAsync(int projectId, CancellationToken ct = default);
    Task<int> RemoveAsync(int projectId, int materialId, int authUserId, CancellationToken ct = default);
}
