using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class UserDropoffRepository : BaseRepository, IUserDropoffRepository
{
    public UserDropoffRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> LogAsync(int userId, int dropoffSiteId, int? materialId, string? quantityText, DateTime droppedAtUtc, CancellationToken ct = default) =>
        WithConnection(conn => conn.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO app.UserDropoff (UserId, DropoffSiteId, MaterialId, QuantityText, DroppedAtUtc)
                  VALUES (@userId, @dropoffSiteId, @materialId, @quantityText, @droppedAtUtc);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { userId, dropoffSiteId, materialId, quantityText, droppedAtUtc },
                cancellationToken: ct)));

    public Task<IReadOnlyList<UserDropoffView>> ListForUserAsync(int userId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT u.UserDropoffId,
       u.DroppedAtUtc,
       u.QuantityText,
       u.MaterialId,
       s.DropoffSiteId,
       s.Name,
       s.City,
       s.Region,
       s.CountryCode
FROM app.UserDropoff u
JOIN app.DropoffSite s ON s.DropoffSiteId = u.DropoffSiteId
WHERE u.UserId = @userId
ORDER BY u.DroppedAtUtc DESC;";
            var rows = (await conn.QueryAsync<UserDropoffView>(
                new CommandDefinition(sql, new { userId }, cancellationToken: ct))).ToList();
            IReadOnlyList<UserDropoffView> list = rows;
            return list;
        });
}
