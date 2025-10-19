using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using Relyf.Repository.Dapper.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DropoffSitesController : ControllerBase
{
    private readonly IDropoffSiteRepository _sites;
    public DropoffSitesController(IDropoffSiteRepository sites) => _sites = sites;

    public sealed record CreateSiteRequest(
        string Name, string? AddressLine1, string? City, string? Region,
        string? PostalCode, string? CountryCode, string? AcceptedNotes);

    // POST /api/dropoffsites
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSiteRequest req, CancellationToken ct)
    {
        var name = req.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        var id = await _sites.CreateAsync(new DropoffSiteRecord
        {
            Name = name,
            AddressLine1 = req.AddressLine1?.Trim(),
            City = req.City?.Trim(),
            Region = req.Region?.Trim(),
            PostalCode = req.PostalCode?.Trim(),
            CountryCode = req.CountryCode?.Trim(),
            AcceptedNotes = req.AcceptedNotes?.Trim()
        }, ct);

        var created = await _sites.GetAsync(id, ct);
        if (created is null)
        {
            // minimal fallback payload if the re-fetch ever returns null
            return CreatedAtAction(nameof(Get), new { id }, new DropoffSiteRecord
            {
                DropoffSiteId = id,
                Name = name
            });
        }
        return CreatedAtAction(nameof(Get), new { id }, created);

    }

    // GET /api/dropoffsites/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var row = await _sites.GetAsync(id, ct);
        return row is null ? NotFound() : Ok(row);
    }

    // GET /api/dropoffsites?city=&q=
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? city, [FromQuery] string? q, CancellationToken ct = default)
    {
        var rows = await _sites.SearchAsync(city, q, 100, ct);
        return Ok(rows);
    }
}
