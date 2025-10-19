namespace Relyf.Repository.Dapper.Models;

public sealed class IdeaSearchRow
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = "";
    public string Preview { get; init; } = "";
    public int UserId { get; init; }
    public int? ItemId { get; init; }
}
