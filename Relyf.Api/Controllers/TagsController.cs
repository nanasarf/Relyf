using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public TagsController(RelyfDbContext db) => _db = db;

    // Create a tag (name unique)
    public sealed record CreateTagRequest(string Name);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var exists = await _db.Tags.AnyAsync(t => t.Name == name, ct);
        if (exists) return Conflict("Tag already exists.");

        var tag = new Tag { Name = name };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(List), new { }, tag);
    }

    // List all tags
    [HttpGet]
    public async Task<IEnumerable<Tag>> List(CancellationToken ct)
        => await _db.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    // Attach tag(s) to an idea
    public sealed record AttachRequest(int IdeaId, List<string> Tags);

    [HttpPost("attach")]
    public async Task<IActionResult> Attach([FromBody] AttachRequest req, CancellationToken ct)
    {
        if (!await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.IdeaId, ct))
            return BadRequest("IdeaId does not exist.");

        var names = req.Tags?.Where(s => !string.IsNullOrWhiteSpace(s))
                             .Select(s => s.Trim())
                             .Distinct(StringComparer.OrdinalIgnoreCase)
                             .ToList() ?? [];

        if (names.Count == 0) return BadRequest("Provide at least one tag name.");

        // ensure tags exist (create missing)
        var existing = await _db.Tags.Where(t => names.Contains(t.Name)).ToListAsync(ct);
        var missing = names.Except(existing.Select(t => t.Name), StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var m in missing)
            _db.Tags.Add(new Tag { Name = m });

        if (missing.Count > 0) await _db.SaveChangesAsync(ct);

        var allTags = await _db.Tags.Where(t => names.Contains(t.Name)).ToListAsync(ct);

        // attach (idempotent)
        foreach (var t in allTags)
        {
            var already = await _db.IdeaTags.FindAsync(new object[] { req.IdeaId, t.TagId }, ct);
            if (already is null)
                _db.IdeaTags.Add(new IdeaTag { IdeaId = req.IdeaId, TagId = t.TagId });
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // List ideas by tag
    [HttpGet("{tagName}/ideas")]
    public async Task<IActionResult> IdeasByTag(string tagName, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        tagName = (tagName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(tagName)) return BadRequest("tagName is required.");

        var ideas = await _db.IdeaTags
            .AsNoTracking()
            .Join(_db.Tags, it => it.TagId, t => t.TagId, (it, t) => new { it.IdeaId, t.Name })
            .Where(x => x.Name == tagName)
            .Join(_db.AiIdeas, x => x.IdeaId, i => i.IdeaId, (x, i) => new
            {
                i.IdeaId,
                i.Title,
                Preview = i.IdeaText.Length > 140 ? i.IdeaText.Substring(0, 140) + "..." : i.IdeaText
            })
            .OrderByDescending(x => x.IdeaId)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(ct);

        return Ok(ideas);
    }
}
