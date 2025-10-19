namespace Relyf.Repository.Dapper.Models;

public sealed class FeedbackRecord
{
    public int FeedbackId { get; init; }
    public int UserId { get; init; }
    public string TargetType { get; init; } = ""; // "Idea" | "Project" | "App"
    public int? TargetId { get; init; }
    public byte? Rating { get; init; }            // 1..5 or null
    public string? Notes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
