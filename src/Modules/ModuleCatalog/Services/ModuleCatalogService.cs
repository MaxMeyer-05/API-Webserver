using ModuleCatalog.Contracts;
using SharedKernel.Modules;

namespace ModuleCatalog.Services;

/// <summary>
/// Service for managing and retrieving information about registered modules in the system.
/// </summary>
public sealed class ModuleCatalogService : IModuleCatalogService
{
    private readonly EndpointDataSource _endpointDataSource;
    private readonly IEnumerable<IModule> _installedModules;

    public ModuleCatalogService(
        EndpointDataSource endpointDataSource, 
        IEnumerable<IModule> installedModules)
    {
        _endpointDataSource = endpointDataSource;
        _installedModules = installedModules;
    }

    /// <inheritdoc />
    public IReadOnlyList<EndpointDto> GetRegisteredEndpoints()
    {
        return _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(endpoint => new EndpointDto(
                endpoint.RoutePattern.RawText ?? string.Empty,
                endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.ToArray() ?? ["*"],
                endpoint.DisplayName))
            .OrderBy(endpoint => endpoint.Route)
            .ThenBy(endpoint => string.Join(',', endpoint.HttpMethods))
            .ToArray();
    }
    
    /// <inheritdoc />
    public IReadOnlyList<ModuleDto> GetInstalledModules()
    {
        return _installedModules
            .Select(module => new ModuleDto(
                module.Slug,
                module.DisplayName,
                module.Description,
                module.Kind.ToString(),
                module.StaticFileUrlPrefix))
            .OrderBy(module => module.Slug)
            .ToArray();
    }
}