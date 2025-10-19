using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IImageRepository
{
    Task<bool> OwnerExistsAsync(string ownerType, int ownerId);
    Task<int> AddAsync(string ownerType, int ownerId, string source, string url, string? altText);
    Task<IReadOnlyList<ImageRecord>> ListByOwnerAsync(string ownerType, int ownerId);
    Task<int> DeleteAsync(int imageId);
}
