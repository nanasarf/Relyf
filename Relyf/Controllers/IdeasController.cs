using Microsoft.AspNetCore.Mvc;
using Relyf.Service.Interfaces;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IdeasController : ControllerBase
{
    private readonly IUpcycleIdeaService _service;
    public IdeasController(IUpcycleIdeaService service) => _service = service;

    // Only accept 'item' in the GET request
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string item, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(item))
            return BadRequest("Query param 'item' is required and must not be empty.");

        var trimmedItem = item.Trim();
        if (trimmedItem.Length < 2)
            return BadRequest("Query param 'item' must be at least 2 characters.");

        // Use multi-message support for AI response
        var messages = new List<(string Role, string Content)>
        {
            ("user", $"List creative, safe ways to reuse or upcycle: {trimmedItem}. Return as a numbered list.")
        };

        var ideas = await _service.GetIdeasFromMessagesAsync(messages, ct);
        if (string.IsNullOrWhiteSpace(ideas))
            return BadRequest("No ideas generated. Please provide a valid item.");

        return Ok(new { ideas });
    }

    public sealed record ChatTurn(string Role, string Content);
    public sealed record ChatRequest(List<ChatTurn> Messages);
}
