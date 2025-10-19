using Relyf.Repository.Dapper.Models;

public interface IItemRepository
{
    Task<ItemRecord?> GetByIdAsync(int itemId, int authUserId);
    Task<(IReadOnlyList<ItemRecord> Rows, int Total)> ListByUserAsync(int authUserId, int skip, int take, string? orderBy, string? direction, string? search);
    Task<int> CreateAsync(int authUserId, string title, string? description, string? sourceItem);
    Task<int> UpdateAsync(int itemId, int authUserId, string title, string? description, string? sourceItem);
    Task<int> SoftDeleteAsync(int itemId, int authUserId);
}
