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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Location>()
            .HasData(
                // Hamburg
                new Location { ZipCode = "20095", City = "Hamburg - Altstadt" },
                new Location { ZipCode = "20099", City = "Hamburg - St. Georg" },
                new Location { ZipCode = "20144", City = "Hamburg - Eimsbüttel" },
                new Location { ZipCode = "20251", City = "Hamburg - Eppendorf" },
                new Location { ZipCode = "20253", City = "Hamburg - Hoheluft-Ost" },
                new Location { ZipCode = "20354", City = "Hamburg - Neustadt" },
                new Location { ZipCode = "20357", City = "Hamburg - Sternschanze" },
                new Location { ZipCode = "20359", City = "Hamburg - St. Pauli" },
                new Location { ZipCode = "20457", City = "Hamburg - HafenCity" },
                new Location { ZipCode = "21029", City = "Hamburg - Bergedorf" },
                new Location { ZipCode = "21073", City = "Hamburg - Harburg" },
                new Location { ZipCode = "21107", City = "Hamburg - Wilhelmsburg" },
                new Location { ZipCode = "22081", City = "Hamburg - Barmbek-Süd" },
                new Location { ZipCode = "22303", City = "Hamburg - Winterhude" },
                new Location { ZipCode = "22335", City = "Hamburg - Fuhlsbüttel" },
                new Location { ZipCode = "22529", City = "Hamburg - Lokstedt" },
                new Location { ZipCode = "22765", City = "Hamburg - Ottensen" },
                new Location { ZipCode = "22767", City = "Hamburg - Altona-Altstadt" },
                new Location { ZipCode = "22769", City = "Hamburg - Altona-Nord" },

                // Berlin
                new Location { ZipCode = "10115", City = "Berlin - Mitte" },
                new Location { ZipCode = "10117", City = "Berlin - Mitte" },
                new Location { ZipCode = "10243", City = "Berlin - Friedrichshain" },
                new Location { ZipCode = "10435", City = "Berlin - Prenzlauer Berg" },
                new Location { ZipCode = "10585", City = "Berlin - Charlottenburg" },
                new Location { ZipCode = "10785", City = "Berlin - Tiergarten" },
                new Location { ZipCode = "10829", City = "Berlin - Schöneberg" },
                new Location { ZipCode = "10969", City = "Berlin - Kreuzberg" },
                new Location { ZipCode = "12043", City = "Berlin - Neukölln" },
                new Location { ZipCode = "13353", City = "Berlin - Wedding" },

                // München
                new Location { ZipCode = "80331", City = "München - Altstadt" },
                new Location { ZipCode = "80336", City = "München - Ludwigsvorstadt" },
                new Location { ZipCode = "80538", City = "München - Lehel" },
                new Location { ZipCode = "80799", City = "München - Maxvorstadt" },
                new Location { ZipCode = "80802", City = "München - Schwabing" },
                new Location { ZipCode = "81667", City = "München - Haidhausen" },

                // Köln
                new Location { ZipCode = "50667", City = "Köln - Altstadt-Nord" },
                new Location { ZipCode = "50674", City = "Köln - Neustadt-Süd" },
                new Location { ZipCode = "50678", City = "Köln - Altstadt-Süd" },
                new Location { ZipCode = "50823", City = "Köln - Ehrenfeld" },
                new Location { ZipCode = "50931", City = "Köln - Lindenthal" },

                // Frankfurt am Main
                new Location { ZipCode = "60311", City = "Frankfurt am Main - Altstadt" },
                new Location { ZipCode = "60313", City = "Frankfurt am Main - Innenstadt" },
                new Location { ZipCode = "60316", City = "Frankfurt am Main - Ostend" },
                new Location { ZipCode = "60325", City = "Frankfurt am Main - Westend" },
                new Location { ZipCode = "60594", City = "Frankfurt am Main - Sachsenhausen" },

                // Stuttgart & Leipzig
                new Location { ZipCode = "70173", City = "Stuttgart - Mitte" },
                new Location { ZipCode = "70178", City = "Stuttgart - Süd" },
                new Location { ZipCode = "04109", City = "Leipzig - Mitte" },
                new Location { ZipCode = "04275", City = "Leipzig - Südvorstadt" }
            );
    }
}