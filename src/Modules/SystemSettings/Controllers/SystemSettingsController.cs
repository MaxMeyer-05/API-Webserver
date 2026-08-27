using Microsoft.AspNetCore.Mvc;

using SystemSettings.Services;

namespace SystemSettings.Controllers;

[ApiController]
[Tags("SystemSettings")]
[Route("api/system-settings")]
[Produces("application/json")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;

    public SystemSettingsController(ISystemSettingsService systemSettingsService)
    {
        _systemSettingsService = systemSettingsService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetSystemSettings()
    {
        return Ok("System settings endpoint is working.");
    }
}