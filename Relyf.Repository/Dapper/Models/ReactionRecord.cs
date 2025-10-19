namespace Relyf.Repository.Dapper.Models;

public sealed class ReactionRecord
{
    public int ReactionId { get; init; }
    public int UserId { get; init; }
    public string TargetType { get; init; } = "";   // "Idea" | "Project"
    public int TargetId { get; init; }
    public string Kind { get; init; } = "";         // "like" | "upvote" | "helpful"
    public DateTime CreatedAtUtc { get; init; }
}
