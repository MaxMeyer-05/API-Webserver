using ModuleCatalog.Contracts;

namespace ModuleCatalog.Services;

public interface IModuleCatalogService
{
    /// <summary>
    /// Retrieves a list of all registered endpoints in the system, including their associated modules and metadata.
    /// </summary>
    /// <returns>A list of registered endpoints.</returns>
    IReadOnlyList<EndpointDto> GetRegisteredEndpoints();

    /// <summary>
    /// Retrieves a list of all installed modules in the system, including their metadata.
    /// </summary>
    /// <returns>A list of installed modules.</returns>
    IReadOnlyList<ModuleDto> GetInstalledModules();
}