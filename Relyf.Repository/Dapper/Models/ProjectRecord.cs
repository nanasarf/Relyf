namespace Relyf.Repository.Dapper.Models;

public sealed class ProjectRecord
{
    public int ProjectId { get; init; }
    public int? IdeaId { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public string Status { get; init; } = "draft";
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public bool IsDeleted { get; init; }
}
