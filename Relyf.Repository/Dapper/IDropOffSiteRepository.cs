using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IDropoffSiteRepository
{
    Task<int> CreateAsync(DropoffSiteRecord site, CancellationToken ct = default);
    Task<DropoffSiteRecord?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<DropoffSiteRecord>> SearchAsync(string? city, string? q, int take, CancellationToken ct = default);
}
