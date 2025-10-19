using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ProjectMaterialRepository : BaseRepository, IProjectMaterialRepository
{
    public ProjectMaterialRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> UpsertAsync(int projectId, int materialId, string? quantityText, int authUserId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            // ownership check
            const string ownSql = "SELECT COUNT(1) FROM app.Project WHERE ProjectId=@projectId AND UserId=@authUserId AND IsDeleted=0;";
            var owns = await conn.ExecuteScalarAsync<int>(new CommandDefinition(ownSql, new { projectId, authUserId }, cancellationToken: ct));
            if (owns == 0) return 0;

            const string sql = @"
IF EXISTS (SELECT 1 FROM app.ProjectMaterial WHERE ProjectId=@projectId AND MaterialId=@materialId)
    UPDATE app.ProjectMaterial SET QuantityText=@quantityText
    WHERE ProjectId=@projectId AND MaterialId=@materialId;
ELSE
    INSERT INTO app.ProjectMaterial (ProjectId, MaterialId, QuantityText)
    VALUES (@projectId, @materialId, @quantityText);";
            return await conn.ExecuteAsync(new CommandDefinition(sql, new { projectId, materialId, quantityText }, cancellationToken: ct));
        });

    public Task<IReadOnlyList<ProjectMaterialView>> ListAsync(int projectId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT m.MaterialId, m.Name, m.Category, pm.QuantityText
FROM app.ProjectMaterial pm
JOIN app.Material m ON m.MaterialId = pm.MaterialId
WHERE pm.ProjectId = @projectId
ORDER BY m.Name ASC;";
            var rows = (await conn.QueryAsync<ProjectMaterialView>(
                new CommandDefinition(sql, new { projectId }, cancellationToken: ct))).ToList();
            IReadOnlyList<ProjectMaterialView> list = rows;
            return list;
        });

    public Task<int> RemoveAsync(int projectId, int materialId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn =>
        {
            const string sql = @"
DELETE pm
FROM app.ProjectMaterial pm
JOIN app.Project p ON p.ProjectId = pm.ProjectId
WHERE pm.ProjectId=@projectId AND pm.MaterialId=@materialId
  AND p.UserId=@authUserId AND p.IsDeleted=0;";
            return conn.ExecuteAsync(new CommandDefinition(sql, new { projectId, materialId, authUserId }, cancellationToken: ct));
        });
}
