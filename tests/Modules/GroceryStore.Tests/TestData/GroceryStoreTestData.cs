using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Database.Entities;

namespace GroceryStore.Tests.TestData;

public static class GroceryStoreTestData
{
    #region DbContext Fixture

    public static (GroceryStoreDbContext Context, SqliteConnection Connection) CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<GroceryStoreDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GroceryStoreDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    #endregion

    #region Entity Fixtures

    public static Location CreateLocation(string zipCode = "10115", string city = "Berlin") => new()
    {
        ZipCode = zipCode,
        City = city
    };

    public static Supplier CreateSupplier(
        string companyName = "Biohof Nord",
        string zipCode = "10115",
        string email = "kontakt@biohof-nord.de") => new()
    {
        Id = Guid.NewGuid(),
        CompanyName = companyName,
        Role = "supplier",
        Email = email,
        PasswordHash = "hash_secret_123",
        Street = "Dorfstraße",
        HouseNumber = "12",
        ZipCode = zipCode,
        CreatedAtDateTime = DateTime.UtcNow,
        UpdatedAtDateTime = DateTime.UtcNow
    };

    public static Customer CreateCustomer(
        string email = "anna.meier@example.com",
        string zipCode = "10115",
        string firstName = "Anna",
        string lastName = "Meier") => new()
    {
        Id = Guid.NewGuid(),
        Role = "customer",
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        PasswordHash = "user_hash_456",
        BirthDate = new DateOnly(1995, 5, 20),
        Street = "Hauptstraße",
        HouseNumber = "4a",
        ZipCode = zipCode,
        CreatedAtDateTime = DateTime.UtcNow,
        UpdatedAtDateTime = DateTime.UtcNow
    };

    public static Ingredient CreateIngredient(
        Guid supplierId,
        string name = "Bio-Milch",
        decimal netPrice = 1.29m,
        int stock = 50,
        string unit = "Liter") => new()
    {
        Name = name,
        Unit = unit,
        NetPrice = netPrice,
        Stock = stock,
        SupplierId = supplierId,
        Calories = 64m,
        Carbohydrates = 4.8m,
        Protein = 3.4m
    };

    public static Recipe CreateRecipe(
        Guid supplierId,
        string name = "Pfannkuchen",
        int preparationTime = 20) => new()
    {
        Name = name,
        SupplierId = supplierId,
        Instructions = "Alle Zutaten vermengen und in der Pfanne goldbraun anbraten.",
        PreparationTime = preparationTime
    };

    public static Order CreateOrder(
        Guid customerId,
        decimal totalAmount = 25.80m) => new()
    {
        CustomerId = customerId,
        OrderDate = DateTime.UtcNow,
        TotalAmount = totalAmount,
        IsCanceled = false,
        IsCompleted = false
    };

    #endregion
}