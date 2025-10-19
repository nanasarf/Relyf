using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:int}/materials")]
public sealed class ItemMaterialsController : ControllerBase
{
    private readonly IItemMaterialRepository _repo;
    private readonly ILookupRepository _lookup;
    public ItemMaterialsController(IItemMaterialRepository repo, ILookupRepository lookup)
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

    // RENAMED: avoid Swagger type-name collision
    public sealed record ItemMaterialUpsertRequest(int MaterialId, byte? PercentShare);

    // PUT /api/items/{itemId}/materials
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Upsert(int itemId, [FromBody] ItemMaterialUpsertRequest req, CancellationToken ct)
    {
        if (!await _lookup.ItemExistsAsync(itemId, ct)) return BadRequest("Invalid itemId.");
        if (!await _lookup.MaterialExistsAsync(req.MaterialId, ct)) return BadRequest("Invalid materialId.");
        if (req.PercentShare is < 0 or > 100) return BadRequest("PercentShare must be between 0 and 100.");

        var n = await _repo.UpsertAsync(itemId, req.MaterialId, req.PercentShare, GetUserId(), ct);
        return n == 0 ? NotFound("Item not found or not owned by user.") : NoContent();
    }

    // GET /api/items/{itemId}/materials
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(int itemId, CancellationToken ct)
    {
        var list = await _repo.ListAsync(itemId, ct);
        return Ok(list);
    }

    // DELETE /api/items/{itemId}/materials/{materialId}
    [HttpDelete("{materialId:int}")]
    [Authorize]
    public async Task<IActionResult> Remove(int itemId, int materialId, CancellationToken ct)
    {
        var n = await _repo.RemoveAsync(itemId, materialId, GetUserId(), ct);
        return n == 0 ? NotFound() : NoContent();
    }
}
