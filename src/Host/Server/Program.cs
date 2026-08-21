using Serilog;

using GroceryStore;
using ModuleCatalog;
using SharedKernel.Modules;
using SystemSettings;

var builder = WebApplication.CreateSlimBuilder(args);

var modules = new IModule[]
{
    new ModuleCatalogModule(),
    new GroceryStoreModule(),
    new SystemSettingsModule()
};

// Configure services for each module
foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
    builder.Services.AddSingleton(module);
}

// Add health checks
var healthChecks = builder.Services.AddHealthChecks();
foreach (var module in modules)
{
    module.RegisterHealthChecks(healthChecks);
}

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build the application
var app = builder.Build();

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
