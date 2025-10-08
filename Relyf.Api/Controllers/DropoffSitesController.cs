using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DropoffSitesController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public DropoffSitesController(RelyfDbContext db) => _db = db;

    public sealed record CreateSiteRequest(
        string Name, string? AddressLine1, string? City, string? Region,
        string? PostalCode, string? CountryCode, string? AcceptedNotes);

    // POST /api/dropoffsites
    [HttpPost]
    public async Task<ActionResult<DropoffSite>> Create([FromBody] CreateSiteRequest req, CancellationToken ct)
    {
        var site = new DropoffSite
        {
            Name = req.Name.Trim(),
            AddressLine1 = req.AddressLine1?.Trim(),
            City = req.City?.Trim(),
            Region = req.Region?.Trim(),
            PostalCode = req.PostalCode?.Trim(),
            CountryCode = req.CountryCode?.Trim(),
            AcceptedNotes = req.AcceptedNotes?.Trim(),
        };
        if (string.IsNullOrWhiteSpace(site.Name)) return BadRequest("Name is required.");

        _db.DropoffSites.Add(site);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(Get), new { id = site.DropoffSiteId }, site);
    }

    // GET /api/dropoffsites/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DropoffSite>> Get(int id, CancellationToken ct)
    {
        var s = await _db.DropoffSites.AsNoTracking().FirstOrDefaultAsync(x => x.DropoffSiteId == id, ct);
        return s is null ? NotFound() : s;
    }

    // GET /api/dropoffsites?city=&q=
    [HttpGet]
    public async Task<IEnumerable<DropoffSite>> Search([FromQuery] string? city, [FromQuery] string? q, CancellationToken ct = default)
    {
        var query = _db.DropoffSites.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim();
            query = query.Where(x => x.City != null && x.City.Contains(c));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim();
            query = query.Where(x =>
                x.Name.Contains(s) ||
                (x.AcceptedNotes != null && x.AcceptedNotes.Contains(s)));
        }

        return await query.OrderBy(x => x.Name).Take(100).ToListAsync(ct);
    }
}
