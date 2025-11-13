namespace Relyf.Repository.Dapper.Models;

public sealed class SavedIdeaView
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = "";
    public string Preview { get; init; } = "";
    public string? ImageUrl { get; init; }
    public List<string> Tags { get; set; } = new();
    public DateTime SavedAtUtc { get; init; }
}
