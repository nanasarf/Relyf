using Microsoft.AspNetCore.Mvc;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly RelyfDbContext _db;
    private readonly Relyf.Service.Interfaces.ICohereClient _cohere;

    public HealthController(RelyfDbContext db, Relyf.Service.Interfaces.ICohereClient cohere)
    {
        _db = db;
        _cohere = cohere;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dbOk = await _db.Database.CanConnectAsync(ct);

        // Very light Cohere ping: send a tiny prompt; swallow errors into status
        bool cohereOk = true;
        try
        {
            var _ = await _cohere.ChatAsync("ping", ct);
        }
        catch { cohereOk = false; }

        return Ok(new { db = dbOk, cohere = cohereOk, utc = DateTime.UtcNow });
    }
}
