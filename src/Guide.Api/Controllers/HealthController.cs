using Microsoft.AspNetCore.Mvc;

namespace Guide.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", service = "guide-api" });
}
