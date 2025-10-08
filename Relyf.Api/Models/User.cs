namespace Relyf.Api.Models;

public class User
{
    public int UserId { get; set; }        
    public string Email { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? CountryCode { get; set; }
}
