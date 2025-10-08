namespace Relyf.Api.Models;

public class SavedIdea
{
    public int UserId { get; set; }  // PK part -> app.User
    public int IdeaId { get; set; }  // PK part -> app.AiIdea
    public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
}
