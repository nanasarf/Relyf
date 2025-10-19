namespace Relyf.Repository.Dapper.Models;

public sealed class ItemMaterialView
{
    public int MaterialId { get; init; }
    public string Name { get; init; } = "";
    public string? Category { get; init; }
    public byte? PercentShare { get; init; }
}
