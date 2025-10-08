using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserDropoffsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public UserDropoffsController(RelyfDbContext db) => _db = db;

    public sealed record LogDropRequest(int UserId, int DropoffSiteId, int? MaterialId, string? QuantityText, DateTime DroppedAtUtc);

    // POST /api/userdropoffs
    [HttpPost]
    public async Task<IActionResult> Log([FromBody] LogDropRequest req, CancellationToken ct)
    {
        if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct))
            return BadRequest("Invalid UserId.");
        if (!await _db.DropoffSites.AnyAsync(s => s.DropoffSiteId == req.DropoffSiteId, ct))
            return BadRequest("Invalid DropoffSiteId.");
        if (req.MaterialId.HasValue && !await _db.Materials.AnyAsync(m => m.MaterialId == req.MaterialId.Value, ct))
            return BadRequest("Invalid MaterialId.");

        var row = new UserDropoff
        {
            UserId = req.UserId,
            DropoffSiteId = req.DropoffSiteId,
            MaterialId = req.MaterialId,
            QuantityText = req.QuantityText?.Trim(),
            DroppedAtUtc = req.DroppedAtUtc
        };
        _db.UserDropoffs.Add(row);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetForUser), new { userId = req.UserId }, row);
    }

    // GET /api/userdropoffs/user/1
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetForUser(int userId, CancellationToken ct)
    {
        var list = await _db.UserDropoffs
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(_db.DropoffSites, u => u.DropoffSiteId, s => s.DropoffSiteId, (u, s) => new
            {
                u.UserDropoffId,
                u.DroppedAtUtc,
                u.QuantityText,
                u.MaterialId,
                Site = new { s.DropoffSiteId, s.Name, s.City, s.Region, s.CountryCode }
            })
            .OrderByDescending(x => x.DroppedAtUtc)
            .ToListAsync(ct);

        return Ok(list);
    }
}
