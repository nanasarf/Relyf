using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;
using Relyf.Repository.Dapper.Models;
using System.Security.Claims;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // require JWT for all endpoints now
public sealed class ProjectsController : ControllerBase
{
    private readonly ILookupRepository _lookup;
    private readonly IProjectRepository _projects;
    private readonly IProjectStepRepository _steps;
    private readonly ISavedAIIdeaRepository _aiIdeas;

    public ProjectsController(ILookupRepository lookup, IProjectRepository projects, IProjectStepRepository steps, ISavedAIIdeaRepository aiIdeas)
    {
        _lookup = lookup;
        _projects = projects;
        _steps = steps;
        _aiIdeas = aiIdeas;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id not found in token.");
        return userId;
    }

    // DTOs (same signatures you had)
    public sealed record CreateProjectRequest(int UserId, int? IdeaId, int? AiIdeaId, string Title, string? Description);
    public sealed record ProjectDto(int ProjectId, int? IdeaId, int? AiIdeaId, int UserId, string Title, string? Description, string Status, string? ImageUrl);
    public sealed record ProjectWithStepsDto(int ProjectId, int? IdeaId, int? AiIdeaId, int UserId, string Title, string? Description, string Status, List<StepDto> Steps, string? ImageUrl);
    public sealed record StepDto(int ProjectStepId, int StepNumber, string Instruction);
    public sealed record UpsertStepsRequest(List<string> Steps);
    public sealed record UpdateStatusRequest(string Status);
    public sealed record PagedProjectsDto(List<ProjectDto> Results, int Total, int Skip, int Take);

    // POST api/projects
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        var userId = GetUserId(); // ignore req.UserId; trust JWT instead

        if (!await _lookup.UserExistsAsync(userId, ct))
            return BadRequest("Authenticated user not found.");

        if (req.IdeaId.HasValue)
        {
            var owned = await _lookup.IdeaOwnedByUserAsync(req.IdeaId.Value, userId, ct);
            if (!owned) return BadRequest("IdeaId does not exist or is not owned by the current user.");
        }

        if (req.AiIdeaId.HasValue)
        {
            var aiIdea = await _aiIdeas.GetByIdAsync(req.AiIdeaId.Value, userId);
            if (aiIdea is null) return BadRequest("AiIdeaId does not exist or is not owned by the current user.");
        }

        var newId = await _projects.CreateAsync(userId, req.IdeaId, req.AiIdeaId, req.Title, req.Description, ct);
        var proj = await _projects.GetAsync(newId, userId, ct); // should exist

        return CreatedAtAction(nameof(Get), new { id = newId },
            new ProjectDto(proj!.ProjectId, proj.IdeaId, proj.AiIdeaId, proj.UserId, proj.Title, proj.Description, proj.Status, proj.ImageUrl));
    }

    // GET api/projects/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectWithStepsDto>> Get(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var proj = await _projects.GetAsync(id, userId, ct);
        if (proj is null) return NotFound();

        var steps = await _steps.ListAsync(id, userId, ct);
        var dtoSteps = steps.Select(s => new StepDto(s.ProjectStepId, s.StepNumber, s.Instruction)).ToList();

        return new ProjectWithStepsDto(proj.ProjectId, proj.IdeaId, proj.AiIdeaId, proj.UserId, proj.Title, proj.Description, proj.Status, dtoSteps, proj.ImageUrl);
    }

    // GET api/projects (current user's projects, paged)
    [HttpGet]
    public async Task<ActionResult<PagedProjectsDto>> List([FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        var rows = await _projects.ListAsync(userId, skip, take, ct);
        var total = await _projects.CountAsync(userId, ct);
        var list = rows.Select(r => new ProjectDto(r.ProjectId, r.IdeaId, r.AiIdeaId, r.UserId, r.Title, r.Description, r.Status, r.ImageUrl)).ToList();
        return new PagedProjectsDto(list, total, skip, take);
    }

    // GET api/projects/user/{userId} (any user's public projects)
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<PagedProjectsDto>> GetUserProjects(int userId, [FromQuery] int skip = 0, [FromQuery] int take = 20, CancellationToken ct = default)
    {
        if (take <= 0) take = 20;
        if (take > 100) take = 100;

        // Verify user exists
        if (!await _lookup.UserExistsAsync(userId, ct))
            return NotFound(new { error = "User not found" });

        var rows = await _projects.GetUserProjectsAsync(userId, skip, take, ct);
        var total = await _projects.CountUserProjectsAsync(userId, ct);
        var list = rows.Select(r => new ProjectDto(r.ProjectId, r.IdeaId, r.AiIdeaId, r.UserId, r.Title, r.Description, r.Status, r.ImageUrl)).ToList();
        return new PagedProjectsDto(list, total, skip, take);
    }

    // PUT api/projects/{id}/steps
    [HttpPut("{id:int}/steps")]
    public async Task<IActionResult> UpsertSteps(int id, [FromBody] UpsertStepsRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        try
        {
            await _steps.UpsertStepsAsync(id, userId, req.Steps, ct);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return NotFound("Project not found.");
        }
    }

    // PATCH api/projects/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var userId = GetUserId();

        var allowed = new[] { "draft", "in_progress", "completed" };
        if (!allowed.Contains(req.Status)) return BadRequest("Invalid status.");

        var n = await _projects.UpdateStatusAsync(id, userId, req.Status, ct);
        return n == 0 ? NotFound("Project not found.") : NoContent();
    }

    // DELETE api/projects/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        var affected = await _projects.SoftDeleteAsync(id, userId, ct);
        return affected == 0 ? NotFound("Project not found.") : NoContent();
    }
}
