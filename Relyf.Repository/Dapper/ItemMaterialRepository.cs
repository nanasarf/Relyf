using System.Linq;
using Dapper;
using Relyf.Repository.Dapper.Models;
using Relyf.Repository.Infrastructure;

namespace Relyf.Repository.Dapper;

public sealed class ItemMaterialRepository : BaseRepository, IItemMaterialRepository
{
    public ItemMaterialRepository(IDbConnectionFactory factory) : base(factory) { }

    public Task<int> UpsertAsync(int itemId, int materialId, byte? percentShare, int authUserId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            // ownership check
            const string ownSql = "SELECT COUNT(1) FROM app.Item WHERE ItemId=@itemId AND UserId=@authUserId AND IsDeleted = 0;";
            var owns = await conn.ExecuteScalarAsync<int>(new CommandDefinition(ownSql, new { itemId, authUserId }, cancellationToken: ct));
            if (owns == 0) return 0;

            const string sql = @"
IF EXISTS (SELECT 1 FROM app.ItemMaterial WHERE ItemId=@itemId AND MaterialId=@materialId)
    UPDATE app.ItemMaterial SET PercentShare=@percentShare WHERE ItemId=@itemId AND MaterialId=@materialId;
ELSE
    INSERT INTO app.ItemMaterial (ItemId, MaterialId, PercentShare) VALUES (@itemId, @materialId, @percentShare);";
            return await conn.ExecuteAsync(new CommandDefinition(sql, new { itemId, materialId, percentShare }, cancellationToken: ct));
        });

    public Task<IReadOnlyList<ItemMaterialView>> ListAsync(int itemId, CancellationToken ct = default) =>
        WithConnection(async conn =>
        {
            const string sql = @"
SELECT m.MaterialId, m.Name, m.Category, im.PercentShare
FROM app.ItemMaterial im
JOIN app.Material m ON m.MaterialId = im.MaterialId
WHERE im.ItemId = @itemId
ORDER BY m.Name ASC;";
            var rows = (await conn.QueryAsync<ItemMaterialView>(
                new CommandDefinition(sql, new { itemId }, cancellationToken: ct))).ToList();
            IReadOnlyList<ItemMaterialView> list = rows;
            return list;
        });

    public Task<int> RemoveAsync(int itemId, int materialId, int authUserId, CancellationToken ct = default) =>
        WithConnection(conn =>
        {
            const string sql = @"
DELETE im
FROM app.ItemMaterial im
JOIN app.Item i ON i.ItemId = im.ItemId
WHERE im.ItemId=@itemId AND im.MaterialId=@materialId AND i.UserId=@authUserId AND i.IsDeleted=0;";
            return conn.ExecuteAsync(new CommandDefinition(sql, new { itemId, materialId, authUserId }, cancellationToken: ct));
        });
}
