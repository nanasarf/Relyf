using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/materials")]
public sealed class ProjectMaterialsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public ProjectMaterialsController(RelyfDbContext db) => _db = db;

    // RENAMED to avoid schemaId collision with ItemMaterialsController
    public sealed record ProjectMaterialUpsertRequest(int MaterialId, string? QuantityText);

    // PUT /api/projects/{projectId}/materials
    [HttpPut]
    public async Task<IActionResult> Upsert(int projectId, [FromBody] ProjectMaterialUpsertRequest req, CancellationToken ct)
    {
        var projOk = await _db.Projects.AnyAsync(p => p.ProjectId == projectId, ct);
        var matOk = await _db.Materials.AnyAsync(m => m.MaterialId == req.MaterialId, ct);
        if (!projOk || !matOk) return BadRequest("Invalid projectId or materialId.");

        var row = await _db.ProjectMaterials.FindAsync(new object[] { projectId, req.MaterialId }, ct);
        if (row is null)
        {
            _db.ProjectMaterials.Add(new ProjectMaterial
            {
                ProjectId = projectId,
                MaterialId = req.MaterialId,
                QuantityText = req.QuantityText?.Trim()
            });
        }
        else
        {
            row.QuantityText = req.QuantityText?.Trim();
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // GET /api/projects/{projectId}/materials
    [HttpGet]
    public async Task<IActionResult> List(int projectId, CancellationToken ct)
    {
        var list = await _db.ProjectMaterials
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId)
            .Join(_db.Materials, pm => pm.MaterialId, m => m.MaterialId, (pm, m) => new
            {
                m.MaterialId,
                m.Name,
                m.Category,
                pm.QuantityText
            })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return Ok(list);
    }

    // DELETE /api/projects/{projectId}/materials/{materialId}
    [HttpDelete("{materialId:int}")]
    public async Task<IActionResult> Remove(int projectId, int materialId, CancellationToken ct)
    {
        var row = await _db.ProjectMaterials.FindAsync(new object[] { projectId, materialId }, ct);
        if (row is null) return NotFound();
        _db.ProjectMaterials.Remove(row);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
