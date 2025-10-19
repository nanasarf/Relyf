using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/ideas")]
public sealed class IdeaStatsController : ControllerBase
{
    private readonly IIdeaStatsRepository _repo;
    public IdeaStatsController(IIdeaStatsRepository repo) => _repo = repo;

    // GET /api/ideas/{id}/stats
    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> Stats(int id, CancellationToken ct)
    {
        var dto = await _repo.GetIdeaStatsAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // GET /api/ideas/top?take=10
    [HttpGet("top")]
    public async Task<IActionResult> Top([FromQuery] int take = 10, CancellationToken ct = default)
    {
        var rows = await _repo.GetTopIdeasAsync(take, ct);
        return Ok(rows);
    }
}
