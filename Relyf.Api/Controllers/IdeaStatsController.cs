using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/ideas")]
public sealed class IdeaStatsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public IdeaStatsController(RelyfDbContext db) => _db = db;

    // GET /api/ideas/{id}/stats
    public sealed record IdeaStatsDto(int IdeaId, int Likes, int Saves, int Comments);
    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<IdeaStatsDto>> Stats(int id, CancellationToken ct)
    {
        var exists = await _db.AiIdeas.AsNoTracking().AnyAsync(i => i.IdeaId == id, ct);
        if (!exists) return NotFound();

        var likes = await _db.Reactions.CountAsync(r => r.TargetType == "Idea" && r.TargetId == id && r.Kind == "like", ct);
        var saves = await _db.SavedIdeas.CountAsync(s => s.IdeaId == id, ct);
        var comments = await _db.Comments.CountAsync(c => c.TargetType == "Idea" && c.TargetId == id, ct);

        return new IdeaStatsDto(id, likes, saves, comments);
    }

    // GET /api/ideas/top?take=10  (orders by a simple popularity score)
    public sealed record TopIdeaDto(int IdeaId, string Title, int Score, int Likes, int Saves, int Comments);
    [HttpGet("top")]
    public async Task<IEnumerable<TopIdeaDto>> Top([FromQuery] int take = 10, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

        var q =
            from i in _db.AiIdeas.AsNoTracking()
            let likes = _db.Reactions.Count(r =>
                r.TargetType == "Idea" && r.Kind == "like" && r.TargetId == i.IdeaId)
            let saves = _db.SavedIdeas.Count(s => s.IdeaId == i.IdeaId)
            let comments = _db.Comments.Count(c =>
                c.TargetType == "Idea" && c.TargetId == i.IdeaId)
            let score = (likes * 3) + (saves * 2) + comments
            orderby score descending, i.IdeaId descending
            select new TopIdeaDto(i.IdeaId, i.Title, score, likes, saves, comments);

        return await q.Take(take).ToListAsync(ct);
    }

}
