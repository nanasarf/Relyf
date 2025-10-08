using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/admin/logs")]
public sealed class AdminLogsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public AdminLogsController(RelyfDbContext db) => _db = db;

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
        take = Math.Clamp(take, 1, 500);

        var q = _db.ApiRequestLogs.AsNoTracking()
            .Where(x => x.ApiRequestLogId > sinceId &&
                        x.StatusCode >= statusMin && x.StatusCode <= statusMax);

        if (userId is not null)
            q = q.Where(x => x.UserId == userId);

        var rows = await q
            .OrderByDescending(x => x.ApiRequestLogId)
            .Take(take)
            .Select(x => new
            {
                x.ApiRequestLogId,
                x.UserId,
                x.Provider,
                x.Endpoint,
                x.Model,
                x.TokensIn,
                x.TokensOut,
                x.StatusCode,
                x.DurationMs
            })
            .ToListAsync(ct);

        return Ok(rows);
    }

    // GET /api/admin/logs/summary?maxId=0
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] int maxId = 0, CancellationToken ct = default)
    {
        var q = _db.ApiRequestLogs.AsNoTracking();
        if (maxId > 0) q = q.Where(x => x.ApiRequestLogId <= maxId);

        var total = await q.CountAsync(ct);
        var errors = await q.CountAsync(x => x.StatusCode >= 400, ct);
        var avgLatency = total == 0 ? (double?)null
                                    : Math.Round(await q.AverageAsync(x => (double)(x.DurationMs ?? 0), ct), 2);
        var tokensIn = await q.SumAsync(x => (int?)x.TokensIn ?? 0, ct);
        var tokensOut = await q.SumAsync(x => (int?)x.TokensOut ?? 0, ct);

        return Ok(new
        {
            total,
            errors,
            errorRate = total == 0 ? 0 : Math.Round(errors * 100.0 / total, 2),
            avgLatencyMs = avgLatency,
            tokensIn,
            tokensOut
        });
    }

    // GET /api/admin/logs/top-models?take=5
    [HttpGet("top-models")]
    public async Task<IActionResult> TopModels([FromQuery] int take = 5, CancellationToken ct = default)
    {
        var q = _db.ApiRequestLogs.AsNoTracking();

        var byModel = await q
            .GroupBy(x => x.Model ?? "(unknown)")
            .Select(g => new
            {
                model = g.Key,
                calls = g.Count(),
                avgLatencyMs = Math.Round(g.Average(x => (double)(x.DurationMs ?? 0)), 2),
                errorRate = Math.Round(100.0 * g.Count(x => x.StatusCode >= 400) / Math.Max(1, g.Count()), 2)
            })
            .OrderByDescending(x => x.calls)
            .Take(Math.Clamp(take, 1, 20))
            .ToListAsync(ct);

        return Ok(byModel);
    }
}
