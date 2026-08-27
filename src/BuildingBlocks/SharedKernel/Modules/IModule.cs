namespace SharedKernel.Modules;

/// <summary>
/// Represents a module that can be registered with the server.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the unique slug for the module.
    /// </summary>
    /// <remarks>
    /// The slug is used in routing and should be unique across all modules.
    /// </remarks>
    string Slug { get; }

    /// <summary>
    /// Defines the display name for the module.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Defines the description for the module.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Defines the kind of the module, 
    /// indicating whether it is a standard module or a system feature.
    /// </summary>
    ModuleKind Kind { get; }

    /// <summary>
    /// Defines the URL prefix for serving static files associated with the module.
    /// </summary>
    /// <remarks>
    /// This is used to serve static files like images, CSS, and JavaScript for the module.
    /// </remarks>
    string StaticFileUrlPrefix { get; }

    /// <summary>
    /// Configures the services required by the module.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to use for service setup.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Registers health checks for the module.
    /// </summary>
    /// <param name="healthChecksBuilder">The health checks builder to register health checks with.</param>
    void RegisterHealthChecks(IHealthChecksBuilder healthChecksBuilder) { }
}