using Microsoft.AspNetCore.Mvc;

namespace Vendo.FormBuilder.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new
    {
        status = "Healthy",
        service = "Vendo.FormBuilder.Api",
        timestampUtc = DateTime.UtcNow
    });
}
