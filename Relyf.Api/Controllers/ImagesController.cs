using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ImagesController : ControllerBase
{
    private static readonly HashSet<string> AllowedOwners = ["Item", "Idea", "Project"];
    private static readonly HashSet<string> AllowedSources = ["upload", "url", "cloudinary"];

    private readonly RelyfDbContext _db;
    public ImagesController(RelyfDbContext db) => _db = db;

    public sealed record AddImageRequest(string OwnerType, int OwnerId, string Source, string Url, string? AltText);

    // POST /api/images  -> attach image to an owner
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddImageRequest req, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(req.OwnerType)) return BadRequest("OwnerType must be Item, Idea, or Project.");
        if (!AllowedSources.Contains(req.Source)) return BadRequest("Source must be upload, url, or cloudinary.");
        if (string.IsNullOrWhiteSpace(req.Url)) return BadRequest("Url is required.");

        // verify owner exists
        var ok = req.OwnerType switch
        {
            "Item" => await _db.Items.AnyAsync(x => x.ItemId == req.OwnerId, ct),
            "Idea" => await _db.AiIdeas.AnyAsync(x => x.IdeaId == req.OwnerId, ct),
            "Project" => await _db.Projects.AnyAsync(x => x.ProjectId == req.OwnerId, ct),
            _ => false
        };
        if (!ok) return BadRequest("Owner not found.");

        var img = new Image { OwnerType = req.OwnerType, OwnerId = req.OwnerId, Source = req.Source, Url = req.Url, AltText = req.AltText };
        _db.Images.Add(img);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(List), new { ownerType = img.OwnerType, ownerId = img.OwnerId }, img);
    }

    // GET /api/images/{ownerType}/{ownerId}
    [HttpGet("{ownerType}/{ownerId:int}")]
    public async Task<IActionResult> List(string ownerType, int ownerId, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(ownerType)) return BadRequest("Invalid ownerType.");
        var list = await _db.Images.AsNoTracking()
            .Where(i => i.OwnerType == ownerType && i.OwnerId == ownerId)
            .OrderByDescending(i => i.ImageId)
            .ToListAsync(ct);
        return Ok(list);
    }

    // DELETE /api/images/{imageId}
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(int imageId, CancellationToken ct)
    {
        var img = await _db.Images.FirstOrDefaultAsync(i => i.ImageId == imageId, ct);
        if (img is null) return NotFound();
        _db.Images.Remove(img);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
