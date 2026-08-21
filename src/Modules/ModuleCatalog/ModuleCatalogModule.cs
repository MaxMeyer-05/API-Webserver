using ModuleCatalog.Controllers;
using ModuleCatalog.Services;
using SharedKernel.Modules;

namespace ModuleCatalog;

/// <summary>
/// Represents the module catalog feature, 
/// which provides information about registered modules in the system.
/// </summary>
public sealed class ModuleCatalogModule : IModule
{
    public string Slug => "shared-kernel";
    public string DisplayName => "Registered Modules";
    public string? Description => "Displays all registered modules.";
    public ModuleKind Kind => ModuleKind.SystemFeature;
    public string StaticFileUrlPrefix => "api/modules";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
                .AddApplicationPart(typeof(ModuleCatalogController).Assembly);
        services.AddSingleton<IModuleCatalogService, ModuleCatalogService>();
    }
}