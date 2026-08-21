using SharedKernel.Modules;

using SystemSettings.Controllers;
using SystemSettings.Services;

namespace SystemSettings;

/// <summary>
/// Represents the system settings feature, 
/// which provides functionality for managing application-wide settings.
/// </summary>
public sealed class SystemSettingsModule : IModule
{
    public string Slug => "system-settings";
    public string DisplayName => "System Settings";
    public string? Description => "Manages application-wide system settings.";
    public ModuleKind Kind => ModuleKind.SystemFeature;
    public string StaticFileUrlPrefix => "modules/system-settings";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
                .AddApplicationPart(typeof(SystemSettingsController).Assembly);
        services.AddSingleton<ISystemSettingsService, SystemSettingsService>();
    }
}