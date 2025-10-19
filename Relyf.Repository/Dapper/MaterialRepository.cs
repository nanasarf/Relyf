using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class MaterialRepository : BaseRepository, IMaterialRepository
{
    public MaterialRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<IReadOnlyList<MaterialRecord>> SearchAsync(string? search, int take, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 100);
            string where = "";
            object param;

            if (string.IsNullOrWhiteSpace(search))
            {
                param = new { take };
            }
            else
            {
                var s = "%" + EscapeLike(search.Trim()) + "%";
                where = "WHERE Name LIKE @s ESCAPE '\\' OR ISNULL(Category,'') LIKE @s ESCAPE '\\'";
                param = new { s, take };
            }

            var sql = $@"
SELECT TOP (@take) MaterialId, Name, Category, Recyclability, Notes
FROM app.Material
{where}
ORDER BY Name ASC;";

            var rows = (await conn.QueryAsync<MaterialRecord>(
                new CommandDefinition(sql, param, cancellationToken: ct))).ToList();
            IReadOnlyList<MaterialRecord> list = rows;
            return list;
        });

    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = "SELECT COUNT(1) FROM app.Material WHERE Name = @name;";
            return await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { name }, cancellationToken: ct)) > 0;
        });

    public Task<int> CreateAsync(string name, string? category, byte? recyclability, string? notes, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.Material (Name, Category, Recyclability, Notes)
                  VALUES (@name, @category, @recyclability, @notes);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { name, category, recyclability, notes }, cancellationToken: ct)));
}
