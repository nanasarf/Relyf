using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IUserRepository
{
    Task<UserRecord?> GetByIdAsync(int userId);
    Task<UserRecord?> GetByEmailAsync(string email);

    Task<int> CreateAsync(string email, string displayName, string? countryCode);
    Task<int> UpdateAsync(int userId, string email, string displayName, string? countryCode, bool isDeleted);
    Task<int> SoftDeleteAsync(int userId); // set IsDeleted = 1
}
