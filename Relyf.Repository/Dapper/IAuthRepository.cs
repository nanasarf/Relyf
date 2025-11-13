using Relyf.Repository.Dapper.Models;

namespace Relyf.Repository.Dapper;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken ct = default);
    Task<UserAuthRecord?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<UserCredentialRecord?> GetCredentialAsync(int userId, CancellationToken ct = default);

    // Creates User + Credential atomically, returns created user
    Task<UserAuthRecord> CreateUserWithCredentialAsync(
        string email, string userName, string displayName, string? countryCode,
        byte[] passwordHash, byte[] passwordSalt, CancellationToken ct = default);
}
