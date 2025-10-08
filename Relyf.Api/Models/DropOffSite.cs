namespace Relyf.Api.Models;

public class DropoffSite
{
    public int DropoffSiteId { get; set; }  // PK
    public string Name { get; set; } = "";
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }     // state/province
    public string? PostalCode { get; set; }
    public string? CountryCode { get; set; } // 2-letter
    public string? AcceptedNotes { get; set; }
}
