using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/ideas/search")]
public sealed class IdeaSearchController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public IdeaSearchController(RelyfDbContext db) => _db = db;

    // GET /api/ideas/search?q=shirt&tag=home-decor&userId=1&skip=0&take=20
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? tag,
        [FromQuery] int? userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);

        var ideas = _db.AiIdeas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            ideas = ideas.Where(i => i.Title.Contains(s) || i.IdeaText.Contains(s));
        }

        if (userId is not null)
        {
            ideas = ideas.Where(i => i.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagName = tag.Trim();
            ideas =
                from i in ideas
                join it in _db.IdeaTags on i.IdeaId equals it.IdeaId
                join t in _db.Tags on it.TagId equals t.TagId
                where t.Name == tagName
                select i;
        }

        var total = await ideas.CountAsync(ct);

        var results = await ideas
            .OrderByDescending(i => i.IdeaId)
            .Skip(skip)
            .Take(take)
            .Select(i => new
            {
                i.IdeaId,
                i.Title,
                Preview = i.IdeaText.Length > 140 ? i.IdeaText.Substring(0, 140) + "..." : i.IdeaText,
                i.UserId,
                i.ItemId
            })
            .ToListAsync(ct);

        return Ok(new { total, skip, take, results });
    }
}
