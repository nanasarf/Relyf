namespace Relyf.Repository.Dapper;

public interface ILookupRepository
{
    Task<bool> UserExistsAsync(int userId, CancellationToken ct = default);
    Task<bool> ItemExistsAsync(int itemId, CancellationToken ct = default);                  // if you already added earlier
    Task<bool> ItemOwnedByUserAsync(int itemId, int userId, CancellationToken ct = default); // earlier step
    Task<bool> IdeaExistsAsync(int ideaId, CancellationToken ct = default);
    Task<bool> IdeaOwnedByUserAsync(int ideaId, int userId, CancellationToken ct = default);
    Task<bool> ProjectOwnedByUserAsync(int projectId, int userId, CancellationToken ct = default);
    Task<bool> ProjectExistsAsync(int projectId, CancellationToken ct = default);
    Task<bool> MaterialExistsAsync(int materialId, CancellationToken ct = default);
    Task<bool> DropoffSiteExistsAsync(int siteId, CancellationToken ct = default);



}
