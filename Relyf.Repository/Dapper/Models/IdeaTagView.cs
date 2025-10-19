namespace Relyf.Repository.Dapper.Models;

public sealed class IdeaTagView
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = "";
    public string Preview { get; init; } = "";
}
