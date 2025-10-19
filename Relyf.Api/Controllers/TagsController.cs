using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController : ControllerBase
{
    private readonly ITagRepository _tags;
    private readonly ILookupRepository _lookup;

    public TagsController(ITagRepository tags, ILookupRepository lookup)
    {
        _tags = tags;
        _lookup = lookup;
    }

    // Create a tag (name unique)
    public sealed record CreateTagRequest(string Name);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var exists = await _tags.ExistsByNameAsync(name, ct);
        if (exists) return Conflict("Tag already exists.");

        var (tagId, _) = await _tags.CreateIfNotExistsAsync(name, ct);
        // return minimal payload like EF did (you returned the Tag row)
        return CreatedAtAction(nameof(List), new { }, new { TagId = tagId, Name = name });
    }

    // List all tags
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var rows = await _tags.ListAllAsync(ct);
        return Ok(rows);
    }

    // Attach tag(s) to an idea
    public sealed record AttachRequest(int IdeaId, List<string> Tags);

    [HttpPost("attach")]
    public async Task<IActionResult> Attach([FromBody] AttachRequest req, CancellationToken ct)
    {
        if (!await _lookup.IdeaExistsAsync(req.IdeaId, ct))
            return BadRequest("IdeaId does not exist.");

        var names = (req.Tags ?? new()).Where(s => !string.IsNullOrWhiteSpace(s))
                                       .Select(s => s.Trim())
                                       .Distinct(StringComparer.OrdinalIgnoreCase)
                                       .ToList();
        if (names.Count == 0) return BadRequest("Provide at least one tag name.");

        await _tags.AttachToIdeaAsync(req.IdeaId, names, ct);
        return NoContent();
    }

    // List ideas by tag
    [HttpGet("{tagName}/ideas")]
    public async Task<IActionResult> IdeasByTag(string tagName, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var name = (tagName ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("tagName is required.");

        var list = await _tags.IdeasByTagAsync(name, Math.Clamp(take, 1, 100), ct);
        return Ok(list);
    }
}
