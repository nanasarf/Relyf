namespace Relyf.Api.Models;

public class Project
{
    public int ProjectId { get; set; }     // PK
    public int? IdeaId { get; set; }       // FK -> app.AiIdea (nullable) - Community ideas
    public int? AiIdeaId { get; set; }     // FK -> app.AIIdeas (nullable) - AI-generated ideas
    public int UserId { get; set; }        // FK -> app.User
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "draft"; // draft | in_progress | completed
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
