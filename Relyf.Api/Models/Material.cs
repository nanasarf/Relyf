namespace Relyf.Api.Models;

public class Material
{
    public int MaterialId { get; set; }         // PK
    public string Name { get; set; } = "";      // unique
    public string? Category { get; set; }       // plastic, fabric, etc.
    public byte? Recyclability { get; set; }    // 0-100
    public string? Notes { get; set; }
}
