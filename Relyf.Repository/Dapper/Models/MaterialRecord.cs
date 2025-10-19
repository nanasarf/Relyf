namespace Relyf.Repository.Dapper.Models;

public sealed class MaterialRecord
{
    public int MaterialId { get; init; }
    public string Name { get; init; } = "";
    public string? Category { get; init; }
    public byte? Recyclability { get; init; }
    public string? Notes { get; init; }
}
