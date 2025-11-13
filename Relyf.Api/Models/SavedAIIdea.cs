namespace Relyf.Api.Models;

public class AIIdea
{
    public int AiIdeaId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = "";
    public string? Tools { get; set; }
    public string? Steps { get; set; }
    public string? Safety { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
