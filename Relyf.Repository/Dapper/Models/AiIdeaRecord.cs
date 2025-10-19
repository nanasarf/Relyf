namespace Relyf.Repository.Dapper.Models;

public sealed class AiIdeaRecord
{
    public int IdeaId { get; init; }
    public int? CoherePromptId { get; init; }   
    public int ItemId { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = "";
    public string IdeaText { get; init; } = "";
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public bool IsDeleted { get; init; }
}
