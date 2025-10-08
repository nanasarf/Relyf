namespace Relyf.Api.Models;

public class ItemMaterial
{
    public int ItemId { get; set; }        // FK -> app.Item
    public int MaterialId { get; set; }    // FK -> app.Material
    public byte? PercentShare { get; set; }  // 0-100 (optional)
}
