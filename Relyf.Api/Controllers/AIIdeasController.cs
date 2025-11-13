using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // require JWT for all endpoints
public sealed class AIIdeasController : ControllerBase
{
    private readonly ISavedAIIdeaRepository _repository;

    public AIIdeasController(ISavedAIIdeaRepository repository)
    {
        _repository = repository;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    // DTOs
    public sealed record CreateAIIdeaRequest(string Title, string? Tools, string? Steps, string? Safety);
    public sealed record UpdateAIIdeaRequest(string Title, string? Tools, string? Steps, string? Safety);
    public sealed record AIIdeaDto(
        int AiIdeaId,
        int UserId,
        string Title,
        string? Tools,
        string? Steps,
        string? Safety,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc
    );
    public sealed record PagedAIIdeasDto(List<AIIdeaDto> Results, int Total, int Skip, int Take);

    // POST api/aiideas - Save an AI idea
    [HttpPost]
    public async Task<ActionResult<AIIdeaDto>> Create([FromBody] CreateAIIdeaRequest req)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");

        var aiIdeaId = await _repository.CreateAsync(userId, req.Title, req.Tools, req.Steps, req.Safety);
        var idea = await _repository.GetByIdAsync(aiIdeaId, userId);

        if (idea is null)
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve created idea.");

        var dto = new AIIdeaDto(
            idea.AiIdeaId,
            idea.UserId,
            idea.Title,
            idea.Tools,
            idea.Steps,
            idea.Safety,
            idea.CreatedAtUtc,
            idea.UpdatedAtUtc
        );

        return CreatedAtAction(nameof(GetById), new { id = aiIdeaId }, dto);
    }

    // GET api/aiideas/{id} - Get a specific AI idea
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AIIdeaDto>> GetById(int id)
    {
        var userId = GetUserId();
        var idea = await _repository.GetByIdAsync(id, userId);

        if (idea is null)
            return NotFound("AI idea not found.");

        var dto = new AIIdeaDto(
            idea.AiIdeaId,
            idea.UserId,
            idea.Title,
            idea.Tools,
            idea.Steps,
            idea.Safety,
            idea.CreatedAtUtc,
            idea.UpdatedAtUtc
        );

        return Ok(dto);
    }

    // GET api/aiideas/user/{userId} - Get user's saved AI ideas
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<PagedAIIdeasDto>> ListByUser(
        int userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var authUserId = GetUserId();

        // Users can only see their own ideas
        if (userId != authUserId)
            return Forbid("You can only view your own AI ideas.");

        if (take <= 0) take = 20;
        if (take > 100) take = 100;
        skip = Math.Max(0, skip);

        var (rows, total) = await _repository.ListByUserAsync(authUserId, skip, take);
        var dtos = rows.Select(r => new AIIdeaDto(
            r.AiIdeaId,
            r.UserId,
            r.Title,
            r.Tools,
            r.Steps,
            r.Safety,
            r.CreatedAtUtc,
            r.UpdatedAtUtc
        )).ToList();

        return Ok(new PagedAIIdeasDto(dtos, total, skip, take));
    }

    // PUT api/aiideas/{id} - Update an AI idea
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAIIdeaRequest req)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest("Title is required.");

        // Verify idea exists and belongs to user
        var idea = await _repository.GetByIdAsync(id, userId);
        if (idea is null)
            return NotFound("AI idea not found.");

        await _repository.UpdateAsync(id, userId, req.Title, req.Tools, req.Steps, req.Safety);
        return NoContent();
    }

    // DELETE api/aiideas/{id} - Delete a saved AI idea
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        // Verify idea exists and belongs to user
        var idea = await _repository.GetByIdAsync(id, userId);
        if (idea is null)
            return NotFound("AI idea not found.");

        await _repository.SoftDeleteAsync(id, userId);
        return NoContent();
    }
}
