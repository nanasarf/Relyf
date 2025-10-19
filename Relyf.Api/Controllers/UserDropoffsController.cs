using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserDropoffsController : ControllerBase
{
    private readonly IUserDropoffRepository _repo;
    private readonly ILookupRepository _lookup;

    public UserDropoffsController(IUserDropoffRepository repo, ILookupRepository lookup)
    {
        _repo = repo;
        _lookup = lookup;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    public sealed record LogDropRequest(int DropoffSiteId, int? MaterialId, string? QuantityText, DateTime DroppedAtUtc);

    // POST /api/userdropoffs
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Log([FromBody] LogDropRequest req, CancellationToken ct)
    {
        var userId = GetUserId();

        if (!await _lookup.DropoffSiteExistsAsync(req.DropoffSiteId, ct))
            return BadRequest("Invalid DropoffSiteId.");
        if (req.MaterialId.HasValue && !await _lookup.MaterialExistsAsync(req.MaterialId.Value, ct))
            return BadRequest("Invalid MaterialId.");

        var id = await _repo.LogAsync(userId, req.DropoffSiteId, req.MaterialId, req.QuantityText?.Trim(), req.DroppedAtUtc, ct);
        return CreatedAtAction(nameof(GetForUser), new { userId }, new { userDropoffId = id });
    }

    // GET /api/userdropoffs/user/{userId}
    [HttpGet("user/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> GetForUser(int userId, CancellationToken ct)
    {
        // Prevent IDOR: caller can only view their own drop-offs
        if (userId != GetUserId()) return Forbid();

        var list = await _repo.ListForUserAsync(userId, ct);
        return Ok(list);
    }
}
