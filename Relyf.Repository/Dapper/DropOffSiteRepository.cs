using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class DropoffSiteRepository : BaseRepository, IDropoffSiteRepository
{
    public DropoffSiteRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> CreateAsync(DropoffSiteRecord site, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.DropoffSite
                  (Name, AddressLine1, City, Region, PostalCode, CountryCode, AcceptedNotes)
                  VALUES (@Name, @AddressLine1, @City, @Region, @PostalCode, @CountryCode, @AcceptedNotes);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                site, cancellationToken: ct)));

    public Task<DropoffSiteRecord?> GetAsync(int id, CancellationToken ct = default) =>
        WithConnection(conn => conn.QuerySingleOrDefaultAsync<DropoffSiteRecord>(
            new CommandDefinition(
                @"SELECT DropoffSiteId, Name, AddressLine1, City, Region, PostalCode, CountryCode, AcceptedNotes
                  FROM app.DropoffSite WHERE DropoffSiteId = @id;",
                new { id }, cancellationToken: ct)));

    public Task<IReadOnlyList<DropoffSiteRecord>> SearchAsync(string? city, string? q, int take, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            take = Math.Clamp(take, 1, 100);

            var where = "1=1";
            var dp = new DynamicParameters();
            dp.Add("take", take);

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = "%" + EscapeLike(city.Trim()) + "%";
                where += " AND ISNULL(City,'') LIKE @c ESCAPE '\\'";
                dp.Add("c", c);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = "%" + EscapeLike(q.Trim()) + "%";
                where += " AND (Name LIKE @s ESCAPE '\\' OR ISNULL(AcceptedNotes,'') LIKE @s ESCAPE '\\')";
                dp.Add("s", s);
            }

            var sql = $@"
SELECT TOP (@take) DropoffSiteId, Name, AddressLine1, City, Region, PostalCode, CountryCode, AcceptedNotes
FROM app.DropoffSite
WHERE {where}
ORDER BY Name ASC;";

            var list = (await conn.QueryAsync<DropoffSiteRecord>(
                new CommandDefinition(sql, dp, cancellationToken: ct))).ToList();
            IReadOnlyList<DropoffSiteRecord> rows = list;
            return rows;
        });
}
