namespace Relyf.Repository.Dapper.Models;

public sealed class IdeaStatsDto
{
    public int IdeaId { get; init; }
    public int Likes { get; init; }
    public int Saves { get; init; }
    public int Comments { get; init; }
}
