namespace Relyf.Repository.Dapper.Models;

public sealed class UserDropoffView
{
    public int UserDropoffId { get; init; }
    public DateTime DroppedAtUtc { get; init; }
    public string? QuantityText { get; init; }
    public int? MaterialId { get; init; }

    public int DropoffSiteId { get; init; }
    public string Name { get; init; } = "";
    public string? City { get; init; }
    public string? Region { get; init; }
    public string CountryCode { get; init; } = "";
}
