namespace Relyf.Api.Models;

public class UserDropoff
{
    public int UserDropoffId { get; set; }  // PK
    public int UserId { get; set; }         // FK -> app.User
    public int DropoffSiteId { get; set; }  // FK -> app.DropoffSite
    public int? MaterialId { get; set; }    // optional FK -> app.Material
    public string? QuantityText { get; set; }
    public DateTime DroppedAtUtc { get; set; } // required
}
