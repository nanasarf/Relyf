namespace Relyf.Api.Models;

public class ProjectMaterial
{
    public int ProjectId { get; set; }     // PK part
    public int MaterialId { get; set; }    // PK part
    public string? QuantityText { get; set; }
}
