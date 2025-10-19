namespace Relyf.Repository.Dapper.Models;

public sealed class DropoffSiteRecord
{
    public int DropoffSiteId { get; init; }
    public string Name { get; init; } = "";
    public string? AddressLine1 { get; init; }
    public string? City { get; init; }
    public string? Region { get; init; }
    public string? PostalCode { get; init; }
    public string? CountryCode { get; init; }
    public string? AcceptedNotes { get; init; }
}
