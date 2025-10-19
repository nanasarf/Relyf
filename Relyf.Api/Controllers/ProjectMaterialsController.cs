using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:int}/materials")]
public sealed class ProjectMaterialsController : ControllerBase
{
    private readonly IProjectMaterialRepository _repo;
    private readonly ILookupRepository _lookup;
    public ProjectMaterialsController(IProjectMaterialRepository repo, ILookupRepository lookup)
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

    // RENAMED to avoid schemaId collision with ItemMaterialsController
    public sealed record ProjectMaterialUpsertRequest(int MaterialId, string? QuantityText);

    // PUT /api/projects/{projectId}/materials
    [HttpPut]
    [Authorize]
    public async Task<IActionResult> Upsert(int projectId, [FromBody] ProjectMaterialUpsertRequest req, CancellationToken ct)
    {
        if (!await _lookup.ProjectExistsAsync(projectId, ct)) return BadRequest("Invalid projectId.");
        if (!await _lookup.MaterialExistsAsync(req.MaterialId, ct)) return BadRequest("Invalid materialId.");

        var n = await _repo.UpsertAsync(projectId, req.MaterialId, req.QuantityText?.Trim(), GetUserId(), ct);
        return n == 0 ? NotFound("Project not found or not owned by user.") : NoContent();
    }

    // GET /api/projects/{projectId}/materials
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(int projectId, CancellationToken ct)
    {
        var list = await _repo.ListAsync(projectId, ct);
        return Ok(list);
    }

    // DELETE /api/projects/{projectId}/materials/{materialId}
    [HttpDelete("{materialId:int}")]
    [Authorize]
    public async Task<IActionResult> Remove(int projectId, int materialId, CancellationToken ct)
    {
        var n = await _repo.RemoveAsync(projectId, materialId, GetUserId(), ct);
        return n == 0 ? NotFound() : NoContent();
    }
}
