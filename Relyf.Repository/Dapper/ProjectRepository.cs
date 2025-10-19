using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ProjectRepository : BaseRepository, IProjectRepository
{
    public ProjectRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(int userId, int? ideaId, string title, string? description, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.Project (UserId, IdeaId, Title, Description, Status, CreatedAtUtc, IsDeleted)
                  VALUES (@userId, @ideaId, @title, @description, N'draft', SYSUTCDATETIME(), 0);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, ideaId, title, description },
                cancellationToken: ct)));

    public Task<ProjectRecord?> GetAsync(int projectId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<ProjectRecord>(
            new CommandDefinition(
                @"SELECT ProjectId, IdeaId, UserId, Title, Description, Status, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                  FROM app.Project
                  WHERE ProjectId = @projectId AND UserId = @authUserId AND IsDeleted = 0;",
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
}
