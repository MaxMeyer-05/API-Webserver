using SharedKernel.Modules;

using GroceryStore.Features.Allergens;
using GroceryStore.Features.Allergens.Interfaces;
using GroceryStore.Features.Categories;
using GroceryStore.Features.Categories.Interfaces;
using GroceryStore.Features.Ingredients;
using GroceryStore.Features.Ingredients.Interfaces;
using GroceryStore.Features.Locations;
using GroceryStore.Features.Locations.Interfaces;
using GroceryStore.Features.Orders;
using GroceryStore.Features.Orders.Interfaces;
using GroceryStore.Features.Recipes;
using GroceryStore.Features.Recipes.Interfaces;
using GroceryStore.Features.Suppliers;
using GroceryStore.Features.Suppliers.Interfaces;
using GroceryStore.Features.Users;
using GroceryStore.Features.Users.Interfaces;

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
        services.AddControllers()
                .AddApplicationPart(typeof(AllergenController).Assembly)
                .AddApplicationPart(typeof(CategoryController).Assembly)
                .AddApplicationPart(typeof(IngredientController).Assembly)
                .AddApplicationPart(typeof(LocationController).Assembly)
                .AddApplicationPart(typeof(OrderController).Assembly)
                .AddApplicationPart(typeof(RecipeController).Assembly)
                .AddApplicationPart(typeof(SupplierController).Assembly)
                .AddApplicationPart(typeof(UserController).Assembly);

            services.AddSingleton<IAllergenMapper, AllergenMapper>();
            services.AddSingleton<ICategoryMapper, CategoryMapper>();
            services.AddSingleton<IIngredientMapper, IngredientMapper>();
            services.AddSingleton<ILocationMapper, LocationMapper>();
            services.AddSingleton<IOrderMapper, OrderMapper>();
            services.AddSingleton<IRecipeMapper, RecipeMapper>();
            services.AddSingleton<ISupplierMapper, SupplierMapper>();
            services.AddSingleton<IUserMapper, UserMapper>();

            services.AddScoped<IAllergenRepository, AllergenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<ILocationRepository, LocationRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<AllergenService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<IngredientService>();
            services.AddScoped<LocationService>();
            services.AddScoped<OrderService>();
            services.AddScoped<RecipeService>();
            services.AddScoped<SupplierService>();
            services.AddScoped<UserService>();
    }
}