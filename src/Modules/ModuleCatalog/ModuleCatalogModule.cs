using SharedKernel.Modules;

namespace ModuleCatalog;

public sealed class ModuleCatalogModule : IModule
{
    public string Slug => "shared-kernel";
    public string DisplayName => "Registered Modules";
    public string? Description => "Displays all registered modules.";
    public ModuleKind Kind => ModuleKind.SystemFeature;
    public string StaticFileUrlPrefix => "api/modules";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/modules")
                             .WithTags("Modules");

        group.MapGet("/", (IEnumerable<IModule> installedModules) =>
        {
            var result = installedModules.Select(m => new ModuleDto(
                m.Slug,
                m.DisplayName,
                m.Description,
                m.Kind.ToString(),
                m.StaticFileUrlPrefix
            ));

            return Results.Ok(result);
        })
        .WithName("GetInstalledModules")
        .WithSummary("Liefert alle registrierten Module für das Hub-Dashboard.")
        .Produces<IEnumerable<ModuleDto>>(StatusCodes.Status200OK);
    }
}