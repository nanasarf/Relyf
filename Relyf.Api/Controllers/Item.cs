using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // require JWT; Swagger "Authorize" button -> paste raw token (no Bearer, no quotes)
public sealed class ItemsController : ControllerBase
{
    private readonly IItemRepository _items;

    public ItemsController(IItemRepository items) => _items = items;

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    // ----- DTOs -----
    public sealed record CreateItemDto(
        [Required, StringLength(120)] string Title,
        [StringLength(4000)] string? Description,
        [StringLength(120)] string? SourceItem);

    // GET: /api/items
    // Supports paging/sorting/search via query
    // Example: /api/items?skip=0&take=20&orderBy=created&direction=desc&search=denim
    [HttpGet]
    public async Task<IActionResult> GetAll(int skip = 0, int take = 20, string? orderBy = null, string? direction = null, string? search = null)
    {
        var userId = GetUserId();
        var (rows, total) = await _items.ListByUserAsync(userId, skip, take, orderBy, direction, search);
        return Ok(new { total, rows });
    }

    // POST: /api/items
    // Always creates for the current user (ignores any incoming userId)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        var id = await _items.CreateAsync(userId, dto.Title, dto.Description, dto.SourceItem);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // GET: /api/items/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        var row = await _items.GetByIdAsync(id, userId);
        return row is null ? NotFound() : Ok(row);
    }
}
