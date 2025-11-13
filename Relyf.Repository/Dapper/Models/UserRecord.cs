namespace Relyf.Repository.Dapper.Models;

public sealed class UserRecord
{
    public int UserId { get; init; }
    public string Email { get; init; } = "";
    public string UserName { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? CountryCode { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public bool IsDeleted { get; init; }
}
