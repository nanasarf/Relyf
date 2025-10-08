using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MaterialsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public MaterialsController(RelyfDbContext db) => _db = db;

    // GET /api/materials?search=cotton
    [HttpGet]
    public async Task<IEnumerable<Material>> Search([FromQuery] string? search, CancellationToken ct)
    {
        var q = _db.Materials.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(m => m.Name.Contains(s) || (m.Category != null && m.Category.Contains(s)));
        }
        return await q.OrderBy(m => m.Name).Take(100).ToListAsync(ct);
    }

    // POST /api/materials  -> create a new catalog material
    public sealed record CreateMaterialRequest(string Name, string? Category, byte? Recyclability, string? Notes);

    [HttpPost]
    public async Task<ActionResult<Material>> Create([FromBody] CreateMaterialRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var exists = await _db.Materials.AnyAsync(m => m.Name == name, ct);
        if (exists) return Conflict("Material already exists.");

        var mat = new Material
        {
            Name = name,
            Category = req.Category?.Trim(),
            Recyclability = req.Recyclability,
            Notes = req.Notes?.Trim()
        };
        _db.Materials.Add(mat);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Search), new { search = mat.Name }, mat);
    }
}
