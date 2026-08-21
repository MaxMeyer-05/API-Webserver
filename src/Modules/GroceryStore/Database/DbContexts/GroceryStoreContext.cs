using Microsoft.EntityFrameworkCore;
using GroceryStore.Database.Entities;

namespace GroceryStore.Database.DbContexts;
public class GroceryStoreDbContext : DbContext
{
    public DbSet<Allergen> Allergens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<User> Users { get; set; }

    public GroceryStoreDbContext(DbContextOptions<GroceryStoreDbContext> options) 
        : base(options)
    {
    }
}