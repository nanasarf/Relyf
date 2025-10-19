using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class AiIdeaRepository : BaseRepository, IAiIdeaRepository
{
    public AiIdeaRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<AiIdeaRecord?> GetByIdAsync(int ideaId, int authUserId) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<AiIdeaRecord>(
            @"SELECT IdeaId, CoherePromptId, ItemId, UserId, Title, IdeaText,
                     CreatedAtUtc, UpdatedAtUtc, IsDeleted
              FROM app.AiIdea
              WHERE IdeaId = @ideaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { ideaId, authUserId }));

    public Task<int> CreateAsync(int authUserId, int itemId, string title, string ideaText, int? coherePromptId) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            @"INSERT INTO app.AiIdea (CoherePromptId, ItemId, UserId, Title, IdeaText, CreatedAtUtc, IsDeleted)
              VALUES (@coherePromptId, @itemId, @authUserId, @title, @ideaText, SYSUTCDATETIME(), 0);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            new { coherePromptId, itemId, authUserId, title, ideaText }));

    public Task<int> UpdateAsync(int ideaId, int authUserId, string title, string ideaText) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.AiIdea
              SET Title = @title,
                  IdeaText = @ideaText,
                  UpdatedAtUtc = SYSUTCDATETIME()
              WHERE IdeaId = @ideaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { ideaId, authUserId, title, ideaText }));

    public Task<int> SoftDeleteAsync(int ideaId, int authUserId) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.AiIdea
              SET IsDeleted = 1, UpdatedAtUtc = SYSUTCDATETIME()
              WHERE IdeaId = @ideaId AND UserId = @authUserId AND IsDeleted = 0;",
            new { ideaId, authUserId }));

    public Task<(IReadOnlyList<AiIdeaRecord> Rows, int Total)> ListByUserAsync(
        int authUserId, int skip, int take, string? orderBy, string? direction, string? search)
        => ListCore(where: "UserId = @authUserId AND IsDeleted = 0",
                    extraParams: new { authUserId },
                    skip, take, orderBy, direction, search);

    public Task<(IReadOnlyList<AiIdeaRecord> Rows, int Total)> ListByItemAsync(
        int authUserId, int itemId, int skip, int take, string? orderBy, string? direction, string? search)
        => ListCore(where: "UserId = @authUserId AND ItemId = @itemId AND IsDeleted = 0",
                    extraParams: new { authUserId, itemId },
                    skip, take, orderBy, direction, search);

    // ---- shared list logic with safe ORDER BY + LIKE ----
    private Task<(IReadOnlyList<AiIdeaRecord> Rows, int Total)> ListCore(
        string where, object extraParams, int skip, int take, string? orderBy, string? direction, string? search)
        => WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 100);
            skip = Math.Max(0, skip);

            string col = (orderBy?.ToLowerInvariant()) switch
            {
                "title" => "Title",
                "updated" => "UpdatedAtUtc",
                "created" => "CreatedAtUtc",
                _ => "IdeaId"
            };
            string dir = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            string? term = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                term = "%" + EscapeLike(search.Trim()) + "%";
                where += " AND (Title LIKE @term ESCAPE '\\' OR ISNULL(IdeaText,'') LIKE @term ESCAPE '\\')";
            }

            var countSql = $"SELECT COUNT(1) FROM app.AiIdea WHERE {where};";
            var listSql =
$@"SELECT IdeaId, CoherePromptId, ItemId, UserId, Title, IdeaText, CreatedAtUtc, UpdatedAtUtc, IsDeleted
   FROM app.AiIdea
   WHERE {where}
   ORDER BY {col} {dir}
   OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

            var param = Merge(extraParams, new { term, skip, take });
            var total = await conn.ExecuteScalarAsync<int>(countSql, param);
            var rowsList = (await conn.QueryAsync<AiIdeaRecord>(listSql, param)).ToList();
            IReadOnlyList<AiIdeaRecord> rows = rowsList;
            return (rows, total);
        });

    private static string EscapeLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");

    // Merge two anonymous objects into a single Expando for Dapper params
    private static object Merge(object a, object b)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in a.GetType().GetProperties()) dict[p.Name] = p.GetValue(a);
        foreach (var p in b.GetType().GetProperties()) dict[p.Name] = p.GetValue(b);
        return dict;
    }
}
