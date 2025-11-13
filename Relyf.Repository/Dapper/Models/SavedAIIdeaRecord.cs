namespace Relyf.Repository.Dapper.Models;

public sealed class SavedAIIdeaRecord
{
    public int AiIdeaId { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = "";
    public string? Tools { get; init; }
    public string? Steps { get; init; }
    public string? Safety { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public bool IsDeleted { get; init; }
}
