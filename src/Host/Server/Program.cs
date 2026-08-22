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

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
