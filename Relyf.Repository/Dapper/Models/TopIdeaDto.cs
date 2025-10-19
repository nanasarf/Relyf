namespace Relyf.Repository.Dapper.Models;

public sealed class TopIdeaDto
{
    public int IdeaId { get; init; }
    public string Title { get; init; } = "";
    public int Score { get; init; }
    public int Likes { get; init; }
    public int Saves { get; init; }
    public int Comments { get; init; }
}
