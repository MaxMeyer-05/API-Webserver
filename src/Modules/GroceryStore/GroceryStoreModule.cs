using SharedKernel.Modules;

namespace GroceryStore;

public sealed class GroceryStoreModule : IModule
{
    public string Slug => "grocery-store";
    public string DisplayName => "Grocery Store";
    public string? Description => "Manages grocery store operations.";
    public ModuleKind Kind => ModuleKind.Standard;
    public string StaticFileUrlPrefix => "modules/grocery-store";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
    }
}