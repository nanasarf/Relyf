namespace Relyf.Api.Models;

public class Comment
{
    public int CommentId { get; set; }           // PK
    public int UserId { get; set; }              // FK -> app.User
    public string TargetType { get; set; } = "Idea"; // 'Idea' | 'Project'
    public int TargetId { get; set; }            // IdeaId or ProjectId
    public string Body { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }   // DB default preferred
}
