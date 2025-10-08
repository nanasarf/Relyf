using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:int}/materials")]
public sealed class ItemMaterialsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public ItemMaterialsController(RelyfDbContext db) => _db = db;

    // RENAMED: avoid Swagger type-name collision
    public sealed record ItemMaterialUpsertRequest(int MaterialId, byte? PercentShare);

    // PUT /api/items/{itemId}/materials
    [HttpPut]
    public async Task<IActionResult> Upsert(int itemId, [FromBody] ItemMaterialUpsertRequest req, CancellationToken ct)
    {
        var itemOk = await _db.Items.AnyAsync(i => i.ItemId == itemId, ct);
        var matOk = await _db.Materials.AnyAsync(m => m.MaterialId == req.MaterialId, ct);
        if (!itemOk || !matOk) return BadRequest("Invalid itemId or materialId.");

        if (req.PercentShare is < 0 or > 100)
            return BadRequest("PercentShare must be between 0 and 100.");

        var row = await _db.ItemMaterials.FindAsync(new object[] { itemId, req.MaterialId }, ct);
        if (row is null)
        {
            row = new ItemMaterial { ItemId = itemId, MaterialId = req.MaterialId, PercentShare = req.PercentShare };
            _db.ItemMaterials.Add(row);
        }
        else
        {
            row.PercentShare = req.PercentShare;
        }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/items/{itemId}/materials
    [HttpGet]
    public async Task<IActionResult> List(int itemId, CancellationToken ct)
    {
        var list = await _db.ItemMaterials.AsNoTracking()
            .Where(x => x.ItemId == itemId)
            .Join(_db.Materials, im => im.MaterialId, m => m.MaterialId, (im, m) => new
            {
                m.MaterialId,
                m.Name,
                m.Category,
                im.PercentShare
            })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return Ok(list);
    }

    // DELETE /api/items/{itemId}/materials/{materialId}
    [HttpDelete("{materialId:int}")]
    public async Task<IActionResult> Remove(int itemId, int materialId, CancellationToken ct)
    {
        var row = await _db.ItemMaterials.FindAsync(new object[] { itemId, materialId }, ct);
        if (row is null) return NotFound();
        _db.ItemMaterials.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
