namespace Relyf.Repository.Dapper.Models;

public sealed class UserAuthRecord
{
    public int UserId { get; init; }
    public string Email { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? CountryCode { get; init; }
}

public sealed class UserCredentialRecord
{
    public int UserId { get; init; }
    public byte[] PasswordHash { get; init; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; init; } = Array.Empty<byte>();
}
