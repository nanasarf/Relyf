namespace Relyf.Repository.Dapper.Models;

public sealed class CommentRecord
{
    public int CommentId { get; init; }
    public int UserId { get; init; }
    public string TargetType { get; init; } = ""; // "Idea" | "Project"
    public int TargetId { get; init; }
    public string Body { get; init; } = "";
    public DateTime CreatedAtUtc { get; init; }
}
