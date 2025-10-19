using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/ideas/search")]
public sealed class IdeaSearchController : ControllerBase
{
    private readonly IIdeaSearchRepository _repo;
    public IdeaSearchController(IIdeaSearchRepository repo) => _repo = repo;

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
        var (rows, total) = await _repo.SearchAsync(q, tag, userId, skip, take, ct);
        return Ok(new { total, skip = Math.Max(0, skip), take = Math.Clamp(take, 1, 100), results = rows });
    }
}
