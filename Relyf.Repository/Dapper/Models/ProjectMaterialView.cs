namespace Relyf.Repository.Dapper.Models;

public sealed class ProjectMaterialView
{
    public int MaterialId { get; init; }
    public string Name { get; init; } = "";
    public string? Category { get; init; }
    public string? QuantityText { get; init; }
}
