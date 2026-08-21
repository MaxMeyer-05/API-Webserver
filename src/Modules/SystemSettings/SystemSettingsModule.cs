using SharedKernel.Modules;

namespace SystemSettings;

public sealed class SystemSettingsModule : IModule
{
    public string Slug => "system-settings";
    public string DisplayName => "System Settings";
    public string? Description => "Manages application-wide system settings.";
    public ModuleKind Kind => ModuleKind.SystemFeature;
    public string StaticFileUrlPrefix => "modules/system-settings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
    }
}