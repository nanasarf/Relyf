using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class SavedAIIdeaRepository : BaseRepository, ISavedAIIdeaRepository
{
    public SavedAIIdeaRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<SavedAIIdeaRecord?> GetByIdAsync(int aiIdeaId, int authUserId) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<SavedAIIdeaRecord>(
            @"SELECT AiIdeaId, UserId, Title, Tools, Steps, Safety, CreatedAtUtc, UpdatedAtUtc, IsDeleted
              FROM app.AIIdeas
              WHERE AiIdeaId = @aiIdeaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { aiIdeaId, authUserId }));

    public Task<(IReadOnlyList<SavedAIIdeaRecord> Rows, int Total)> ListByUserAsync(int authUserId, int skip, int take) =>
        WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 100);
            skip = Math.Max(0, skip);

            var countSql = @"SELECT COUNT(1) FROM app.AIIdeas WHERE UserId = @authUserId AND IsDeleted = 0;";
            var listSql = @"SELECT AiIdeaId, UserId, Title, Tools, Steps, Safety, CreatedAtUtc, UpdatedAtUtc, IsDeleted
                           FROM app.AIIdeas
                           WHERE UserId = @authUserId AND IsDeleted = 0
                           ORDER BY CreatedAtUtc DESC
                           OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

            var total = await conn.ExecuteScalarAsync<int>(countSql, new { authUserId });
            var rowsList = (await conn.QueryAsync<SavedAIIdeaRecord>(listSql, new { authUserId, skip, take })).ToList();
            
            IReadOnlyList<SavedAIIdeaRecord> rows = rowsList;
            return (rows, total);
        });

    public Task<int> CreateAsync(int userId, string title, string? tools, string? steps, string? safety) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            @"INSERT INTO app.AIIdeas (UserId, Title, Tools, Steps, Safety, CreatedAtUtc, IsDeleted)
              VALUES (@userId, @title, @tools, @steps, @safety, SYSUTCDATETIME(), 0);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            new { userId, title, tools, steps, safety }));

    public Task<int> UpdateAsync(int aiIdeaId, int authUserId, string title, string? tools, string? steps, string? safety) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.AIIdeas
              SET Title = @title,
                  Tools = @tools,
                  Steps = @steps,
                  Safety = @safety,
                  UpdatedAtUtc = SYSUTCDATETIME()
              WHERE AiIdeaId = @aiIdeaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { aiIdeaId, authUserId, title, tools, steps, safety }));

    public Task<int> SoftDeleteAsync(int aiIdeaId, int authUserId) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.AIIdeas
              SET IsDeleted = 1, UpdatedAtUtc = SYSUTCDATETIME()
              WHERE AiIdeaId = @aiIdeaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { aiIdeaId, authUserId }));
}
