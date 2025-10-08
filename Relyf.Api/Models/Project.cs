namespace Relyf.Api.Models;

public class Project
{
    public int ProjectId { get; set; }     // PK
    public int? IdeaId { get; set; }       // FK -> app.AiIdea (nullable)
    public int UserId { get; set; }        // FK -> app.User
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "draft"; // draft | in_progress | completed
}
