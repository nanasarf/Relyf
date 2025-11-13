using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ProjectRepository : BaseRepository, IProjectRepository
{
    public ProjectRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, int? ideaId, int? aiIdeaId, string title, string? description, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.Project (UserId, IdeaId, AiIdeaId, Title, Description, Status, CreatedAtUtc, IsDeleted)
                  VALUES (@userId, @ideaId, @aiIdeaId, @title, @description, N'draft', SYSUTCDATETIME(), 0);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, ideaId, aiIdeaId, title, description },
                cancellationToken: ct)));

    public Task<ProjectRecord?> GetAsync(int projectId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<ProjectRecord>(
            new CommandDefinition(
                @"SELECT p.ProjectId, p.IdeaId, p.AiIdeaId, p.UserId, p.Title, p.Description, p.Status, 
                         p.CreatedAtUtc, p.UpdatedAtUtc, p.IsDeleted,
                         (SELECT TOP 1 i.Url 
                          FROM app.Image i 
                          WHERE i.OwnerType = 'Project' 
                            AND i.OwnerId = p.ProjectId 
                          ORDER BY i.CreatedAtUtc ASC) AS ImageUrl
                  FROM app.Project p
                  WHERE p.ProjectId = @projectId AND p.UserId = @authUserId AND p.IsDeleted = 0;",
                new { projectId, authUserId },
                cancellationToken: ct)));

    public Task<int> UpdateStatusAsync(int projectId, int authUserId, string status, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                @"UPDATE app.Project
                  SET Status = @status, UpdatedAtUtc = SYSUTCDATETIME()
                  WHERE ProjectId = @projectId AND UserId = @authUserId AND IsDeleted = 0;",
                new { projectId, authUserId, status },
                cancellationToken: ct)));

    public Task<IEnumerable<ProjectRecord>> ListAsync(int authUserId, int skip, int take, CancellationToken ct = default) =>
        WithConnection(conn => conn.QueryAsync<ProjectRecord>(
            new CommandDefinition(
                @"SELECT p.ProjectId, p.IdeaId, p.AiIdeaId, p.UserId, p.Title, p.Description, p.Status, 
                         p.CreatedAtUtc, p.UpdatedAtUtc, p.IsDeleted,
                         (SELECT TOP 1 i.Url 
                          FROM app.Image i 
                          WHERE i.OwnerType = 'Project' 
                            AND i.OwnerId = p.ProjectId 
                          ORDER BY i.CreatedAtUtc ASC) AS ImageUrl
                  FROM app.Project p
                  WHERE p.UserId = @authUserId AND p.IsDeleted = 0
                  ORDER BY p.CreatedAtUtc DESC
                  OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;",
                new { authUserId, skip, take },
                cancellationToken: ct)));

    public Task<int> CountAsync(int authUserId, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"SELECT COUNT(*) FROM app.Project WHERE UserId = @authUserId AND IsDeleted = 0;",
                new { authUserId },
                cancellationToken: ct)));

    // Public profile view - get any user's projects
    public Task<IEnumerable<ProjectRecord>> GetUserProjectsAsync(int userId, int skip, int take, CancellationToken ct = default) =>
        WithConnection(conn => conn.QueryAsync<ProjectRecord>(
            new CommandDefinition(
                @"SELECT p.ProjectId, p.IdeaId, p.AiIdeaId, p.UserId, p.Title, p.Description, p.Status, 
                         p.CreatedAtUtc, p.UpdatedAtUtc, p.IsDeleted,
                         (SELECT TOP 1 i.Url 
                          FROM app.Image i 
                          WHERE i.OwnerType = 'Project' 
                            AND i.OwnerId = p.ProjectId 
                          ORDER BY i.CreatedAtUtc ASC) AS ImageUrl
                  FROM app.Project p
                  WHERE p.UserId = @userId AND p.IsDeleted = 0
                  ORDER BY p.CreatedAtUtc DESC
                  OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;",
                new { userId, skip, take },
                cancellationToken: ct)));

    public Task<int> CountUserProjectsAsync(int userId, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"SELECT COUNT(*) FROM app.Project WHERE UserId = @userId AND IsDeleted = 0;",
                new { userId },
                cancellationToken: ct)));

    public Task<int> SoftDeleteAsync(int projectId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteAsync(
            new CommandDefinition(
                @"UPDATE app.Project
                  SET IsDeleted = 1, UpdatedAtUtc = SYSUTCDATETIME()
                  WHERE ProjectId = @projectId AND UserId = @authUserId AND IsDeleted = 0;",
                new { projectId, authUserId },
                cancellationToken: ct)));
}
