using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/admin/logs")]
public sealed class AdminLogsController : ControllerBase
{
    private readonly IAdminLogsRepository _repo;
    public AdminLogsController(IAdminLogsRepository repo) => _repo = repo;

    // GET /api/admin/logs/recent?sinceId=0&statusMin=200&statusMax=599&userId=1&take=50
    [HttpGet("recent")]
    public async Task<IActionResult> Recent(
        [FromQuery] int sinceId = 0,
        [FromQuery] int? userId = null,
        [FromQuery] int statusMin = 200,
        [FromQuery] int statusMax = 599,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var rows = await _repo.GetRecentAsync(sinceId, userId, statusMin, statusMax, take, ct);
        return Ok(rows);
    }

    // GET /api/admin/logs/summary?maxId=0
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] int maxId = 0, CancellationToken ct = default)
    {
        var dto = await _repo.GetSummaryAsync(maxId, ct);
        return Ok(dto);
    }

    // GET /api/admin/logs/top-models?take=5
    [HttpGet("top-models")]
    public async Task<IActionResult> TopModels([FromQuery] int take = 5, CancellationToken ct = default)
    {
        var rows = await _repo.GetTopModelsAsync(take, ct);
        return Ok(rows);
    }
}
