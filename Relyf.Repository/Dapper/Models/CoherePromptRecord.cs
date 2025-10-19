namespace Relyf.Repository.Dapper.Models;

public sealed class CoherePromptRecord
{
    public int CoherePromptId { get; init; }
    public int UserId { get; init; }
    public int? ItemId { get; init; }
    public string? Model { get; init; }
    public decimal? Temperature { get; init; }
    public decimal? TopP { get; init; }
    public string PromptText { get; init; } = "";
    public DateTime CreatedAtUtc { get; init; }
}
