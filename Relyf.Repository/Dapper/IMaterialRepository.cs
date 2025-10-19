using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IMaterialRepository
{
    Task<IReadOnlyList<MaterialRecord>> SearchAsync(string? search, int take, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<int> CreateAsync(string name, string? category, byte? recyclability, string? notes, CancellationToken ct = default);
}
