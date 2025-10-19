using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IProjectStepRepository
{
    Task<IReadOnlyList<ProjectStepRecord>> ListAsync(int projectId, int authUserId, CancellationToken ct = default);
    Task UpsertStepsAsync(int projectId, int authUserId, IEnumerable<string> steps, CancellationToken ct = default);
}
