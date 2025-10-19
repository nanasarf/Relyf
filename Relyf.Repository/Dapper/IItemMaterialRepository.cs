using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IItemMaterialRepository
{
    Task<int> UpsertAsync(int itemId, int materialId, byte? percentShare, int authUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ItemMaterialView>> ListAsync(int itemId, CancellationToken ct = default);
    Task<int> RemoveAsync(int itemId, int materialId, int authUserId, CancellationToken ct = default);
}
