using Microsoft.AspNetCore.Mvc;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    // Removed dependencies to test if DI is the issue
    public HealthController()
    {
    }

    [HttpGet]
    public IActionResult Get()
    {
        // Super simple health check - just return OK without any dependencies
        return Ok(new { status = "healthy", utc = DateTime.UtcNow });
    }
}
