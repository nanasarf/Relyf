namespace Relyf.Api.Models;

public class Reaction
{
    public int ReactionId { get; set; }      // PK (IDENTITY)
    public int UserId { get; set; }          // FK -> app.User
    public string TargetType { get; set; } = "Idea"; // 'Idea' | 'Project'
    public int TargetId { get; set; }        // IdeaId or ProjectId depending on TargetType
    public string Kind { get; set; } = "like";       // 'like' | 'upvote' | 'helpful'
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
