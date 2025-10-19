using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.ComponentModel.DataAnnotations;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users) => _users = users;

    // DTO — keeps the API surface minimal and stable
    public sealed record CreateUserDto(
        [Required, EmailAddress, StringLength(320)] string Email,
        [Required, StringLength(120)] string DisplayName,
        [StringLength(10)] string? CountryCode
    );

    /// <summary>Create a user.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // Create and return the new id
        var newId = await _users.CreateAsync(dto.Email.Trim(), dto.DisplayName.Trim(), dto.CountryCode?.Trim());
        // Optionally re-fetch for return payload
        var created = await _users.GetByIdAsync(newId);
        return CreatedAtAction(nameof(GetById), new { id = newId }, (object?)created ?? new { userId = newId });
    }

    /// <summary>Get a user by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var row = await _users.GetByIdAsync(id);
        return row is null ? NotFound() : Ok(row);
    }
}
