using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProjectsController : ControllerBase
{
    private readonly RelyfDbContext _db;
    public ProjectsController(RelyfDbContext db) => _db = db;

    // Create a project (optionally from an existing idea)
    public sealed record CreateProjectRequest(int UserId, int? IdeaId, string Title, string? Description);
    public sealed record ProjectDto(int ProjectId, int? IdeaId, int UserId, string Title, string? Description, string Status);

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest req, CancellationToken ct)
    {
        if (!await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct))
            return BadRequest("UserId does not exist.");
        if (req.IdeaId.HasValue && !await _db.AiIdeas.AnyAsync(i => i.IdeaId == req.IdeaId.Value, ct))
            return BadRequest("IdeaId does not exist.");

        var p = new Models.Project
        {
            UserId = req.UserId,
            IdeaId = req.IdeaId,
            Title = req.Title,
            Description = req.Description,
            Status = "draft"
        };
        _db.Projects.Add(p);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = p.ProjectId },
            new ProjectDto(p.ProjectId, p.IdeaId, p.UserId, p.Title, p.Description, p.Status));
    }

    // Get a project with steps
    public sealed record ProjectWithStepsDto(
        int ProjectId, int? IdeaId, int UserId, string Title, string? Description, string Status,
        List<StepDto> Steps);
    public sealed record StepDto(int ProjectStepId, int StepNumber, string Instruction);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectWithStepsDto>> Get(int id, CancellationToken ct)
    {
        var proj = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == id, ct);
        if (proj is null) return NotFound();

        var steps = await _db.ProjectSteps.AsNoTracking()
            .Where(s => s.ProjectId == id)
            .OrderBy(s => s.StepNumber)
            .Select(s => new StepDto(s.ProjectStepId, s.StepNumber, s.Instruction))
            .ToListAsync(ct);

        return new ProjectWithStepsDto(proj.ProjectId, proj.IdeaId, proj.UserId, proj.Title, proj.Description, proj.Status, steps);
    }

    // Add or replace steps (bulk)
    public sealed record UpsertStepsRequest(List<string> Steps); // index = StepNumber-1

    [HttpPut("{id:int}/steps")]
    public async Task<IActionResult> UpsertSteps(int id, [FromBody] UpsertStepsRequest req, CancellationToken ct)
    {
        var projExists = await _db.Projects.AnyAsync(p => p.ProjectId == id, ct);
        if (!projExists) return NotFound("Project not found.");

        var existing = await _db.ProjectSteps.Where(s => s.ProjectId == id).ToListAsync(ct);
        _db.ProjectSteps.RemoveRange(existing);

        int n = 0;
        foreach (var instruction in req.Steps.Where(s => !string.IsNullOrWhiteSpace(s)))
        {
            n++;
            _db.ProjectSteps.Add(new Models.ProjectStep
            {
                ProjectId = id,
                StepNumber = n,
                Instruction = instruction.Trim()
            });
        }
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // Update status (draft -> in_progress -> completed)
    public sealed record UpdateStatusRequest(string Status);

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req, CancellationToken ct)
    {
        var proj = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == id, ct);
        if (proj is null) return NotFound("Project not found.");

        var allowed = new[] { "draft", "in_progress", "completed" };
        if (!allowed.Contains(req.Status)) return BadRequest("Invalid status.");

        proj.Status = req.Status;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
