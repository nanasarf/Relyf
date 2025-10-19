using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ImagesController : ControllerBase
{
    private static readonly HashSet<string> AllowedOwners = new(StringComparer.OrdinalIgnoreCase) { "Item", "Idea", "Project" };
    private static readonly HashSet<string> AllowedSources = new(StringComparer.OrdinalIgnoreCase) { "upload", "url", "cloudinary" };

    private readonly IImageRepository _repo;
    public ImagesController(IImageRepository repo) => _repo = repo;

    public sealed record AddImageRequest(string OwnerType, int OwnerId, string Source, string Url, string? AltText);

    // POST /api/images
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddImageRequest req, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(req.OwnerType)) return BadRequest("OwnerType must be Item, Idea, or Project.");
        if (!AllowedSources.Contains(req.Source)) return BadRequest("Source must be upload, url, or cloudinary.");
        if (string.IsNullOrWhiteSpace(req.Url)) return BadRequest("Url is required.");

        if (!await _repo.OwnerExistsAsync(req.OwnerType, req.OwnerId))
            return BadRequest("Owner not found.");

        var id = await _repo.AddAsync(req.OwnerType, req.OwnerId, req.Source, req.Url, req.AltText);
        return CreatedAtAction(nameof(List), new { ownerType = req.OwnerType, ownerId = req.OwnerId }, new { imageId = id });
    }

    // GET /api/images/{ownerType}/{ownerId}
    [HttpGet("{ownerType}/{ownerId:int}")]
    public async Task<IActionResult> List(string ownerType, int ownerId, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(ownerType)) return BadRequest("Invalid ownerType.");
        var list = await _repo.ListByOwnerAsync(ownerType, ownerId);
        return Ok(list);
    }

    // DELETE /api/images/{imageId}
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(int imageId, CancellationToken ct)
    {
        var n = await _repo.DeleteAsync(imageId);
        return n == 0 ? NotFound() : NoContent();
    }
}
