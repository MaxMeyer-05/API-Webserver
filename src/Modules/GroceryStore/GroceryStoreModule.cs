using SharedKernel.Modules;

namespace GroceryStore;

/// <summary>
/// Represents the grocery store module, 
/// which manages grocery store operations.
/// </summary>
public sealed class GroceryStoreModule : IModule
{
    public string Slug => "grocery-store";
    public string DisplayName => "Grocery Store";
    public string? Description => "Manages grocery store operations.";
    public ModuleKind Kind => ModuleKind.Standard;
    public string StaticFileUrlPrefix => "modules/grocery-store";

    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }
}