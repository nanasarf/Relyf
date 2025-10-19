namespace Relyf.Repository.Dapper.Models;

public sealed class SavedIdeaView
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = "";
    public string Preview { get; init; } = "";
    public DateTime SavedAtUtc { get; init; }
}
