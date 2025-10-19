using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IUserDropoffRepository
{
    Task<int> LogAsync(int userId, int dropoffSiteId, int? materialId, string? quantityText, DateTime droppedAtUtc, CancellationToken ct = default);
    Task<IReadOnlyList<UserDropoffView>> ListForUserAsync(int userId, CancellationToken ct = default);
}
