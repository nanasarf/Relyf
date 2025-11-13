using Microsoft.AspNetCore.Mvc;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Backend is working!", utc = DateTime.UtcNow });
    }
}