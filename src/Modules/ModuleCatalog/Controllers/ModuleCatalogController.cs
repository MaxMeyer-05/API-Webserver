using Microsoft.AspNetCore.Mvc;

using ModuleCatalog.Contracts;
using ModuleCatalog.Services;

namespace ModuleCatalog.Controllers;

/// <summary>
/// Controller for managing and retrieving information about registered modules in the system.
/// </summary>
[ApiController]
[Route("api/modules")]
[Tags("ModuleCatalog")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public sealed class ModuleCatalogController : ControllerBase
{
    private readonly IModuleCatalogService _moduleCatalogService;

    public ModuleCatalogController(IModuleCatalogService moduleCatalogService)
    {
        _moduleCatalogService = moduleCatalogService;
    }

    /// <summary>
    /// Retrieves a list of all registered endpoints in the system, including their associated modules and metadata.
    /// </summary>
    /// <returns>A list of registered endpoints.</returns>
    /// <response code="200">Returns a list of registered endpoints.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("endpoints", Name = "GetRegisteredEndpoints")]
    [ProducesResponseType<IEnumerable<EndpointDto>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EndpointDto>> GetRegisteredEndpoints()
    {
        return Ok(_moduleCatalogService.GetRegisteredEndpoints());
    }

    /// <summary>
    /// Retrieves a list of all installed modules in the system, including their metadata.
    /// </summary>
    /// <returns>A list of installed modules.</returns>
    /// <response code="200">Returns a list of installed modules.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("installed", Name = "GetInstalledModules")]
    [ProducesResponseType<IEnumerable<ModuleDto>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ModuleDto>> GetInstalledModules()
    {
        return Ok(_moduleCatalogService.GetInstalledModules());
    }
}