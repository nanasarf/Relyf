using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MaterialsController : ControllerBase
{
    private readonly IMaterialRepository _materials;
    public MaterialsController(IMaterialRepository materials) => _materials = materials;

    // GET /api/materials?search=cotton
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? search, CancellationToken ct)
    {
        var rows = await _materials.SearchAsync(search, 100, ct);
        return Ok(rows);
    }

    public sealed record CreateMaterialRequest(string Name, string? Category, byte? Recyclability, string? Notes);

    // POST /api/materials
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        if (await _materials.ExistsByNameAsync(name, ct))
            return Conflict("Material already exists.");

        var id = await _materials.CreateAsync(name, req.Category?.Trim(), req.Recyclability, req.Notes?.Trim(), ct);
        // return something close to what you had
        return CreatedAtAction(nameof(Search), new { search = name }, new { MaterialId = id, Name = name });
    }
}
