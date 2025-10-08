using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Relyf.Api;
using Relyf.Api.Models;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public ItemsController(RelyfDbContext db) => _db = db;

    // GET: /api/items
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetAll()
        => await _db.Items.AsNoTracking().ToListAsync();

    // POST: /api/items
    [HttpPost]
    public async Task<ActionResult<Item>> Create(Item dto)
    {
        // simple guard: must reference an existing User
        var userExists = await _db.Users.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists) return BadRequest("UserId does not exist.");

        _db.Items.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = dto.ItemId }, dto);
    }

    // GET: /api/items/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Item>> GetById(int id)
    {
        var item = await _db.Items.FindAsync(id);
        return item is null ? NotFound() : item;
    }
}
