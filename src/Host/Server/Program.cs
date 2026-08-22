using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;

using Server.Database.DbContexts;
using Server.Extensions;

var builder = WebApplication.CreateSlimBuilder(args);

var modules = builder.AddServerServices();

// Build the application
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var groceryStoreDbContext = scope.ServiceProvider.GetRequiredService<GroceryStoreDbContext>();
    groceryStoreDbContext.Database.Migrate();

    var serverDbContext = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
    serverDbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

foreach (var module in modules)
{
    app.MapHealthChecks($"/api/health/{module.Slug}", new()
    {
        Predicate = registration => registration.Tags.Contains($"module:{module.Slug}")
    });
}

app.Run();
