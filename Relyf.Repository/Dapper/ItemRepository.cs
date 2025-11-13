using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ItemRepository : BaseRepository, IItemRepository
{
    public ItemRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<ItemRecord?> GetByIdAsync(int itemId, int authUserId) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<ItemRecord>(
            @"SELECT ItemId, UserId, Title, Description, CreatedAtUtc, UpdatedAtUtc, IsDeleted
              FROM app.Item
              WHERE ItemId = @itemId AND UserId = @authUserId AND IsDeleted = 0;",
            new { itemId, authUserId }));

    public Task<int> CreateAsync(int authUserId, string title, string? description, string? sourceItem) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            @"INSERT INTO app.Item (UserId, Title, Description, CreatedAtUtc, IsDeleted)
              VALUES (@authUserId, @title, @description, SYSUTCDATETIME(), 0);
              SELECT CAST(SCOPE_IDENTITY() AS int);",
            new { authUserId, title, description }));

    public Task<int> UpdateAsync(int itemId, int authUserId, string title, string? description, string? sourceItem) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.Item
              SET Title = @title,
                  Description = @description,
                  UpdatedAtUtc = SYSUTCDATETIME()
              WHERE ItemId = @itemId AND UserId = @authUserId AND IsDeleted = 0;",
            new { itemId, authUserId, title, description }));

    public Task<int> SoftDeleteAsync(int itemId, int authUserId) =>
        WithConnection(conn => conn.ExecuteAsync(
            @"UPDATE app.Item
              SET IsDeleted = 1, UpdatedAtUtc = SYSUTCDATETIME()
              WHERE ItemId = @itemId AND UserId = @authUserId AND IsDeleted = 0;",
            new { itemId, authUserId }));

    public Task<(IReadOnlyList<ItemRecord> Rows, int Total)> ListByUserAsync(
        int authUserId, int skip, int take, string? orderBy, string? direction, string? search)
    => WithConnection(async conn =>
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);

        // Whitelist ORDER BY
        string col = (orderBy?.ToLowerInvariant()) switch
        {
            "title" => "Title",
            "updated" => "UpdatedAtUtc",
            "created" => "CreatedAtUtc",
            _ => "ItemId"
        };
        string dir = string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

        string where = "UserId = @authUserId AND IsDeleted = 0";
        string? term = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            term = "%" + EscapeLike(search.Trim()) + "%";
            where += " AND (Title LIKE @term ESCAPE '\\' OR ISNULL(Description,'') LIKE @term ESCAPE '\\')";
        }

        var countSql = $"SELECT COUNT(1) FROM app.Item WHERE {where};";
        var listSql =
$@"SELECT ItemId, UserId, Title, Description, CreatedAtUtc, UpdatedAtUtc, IsDeleted
   FROM app.Item
   WHERE {where}
   ORDER BY {col} {dir}
   OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";

        var param = new { authUserId, term, skip, take };

        var total = await conn.ExecuteScalarAsync<int>(countSql, param);
        var rowsList = (await conn.QueryAsync<ItemRecord>(listSql, param)).ToList();
        IReadOnlyList<ItemRecord> rows = rowsList;

        return (rows, total);
    });

    private static string EscapeLike(string s) =>
        s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[");
}
