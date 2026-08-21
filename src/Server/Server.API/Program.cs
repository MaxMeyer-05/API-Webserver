using Serilog;
using Server.API.Modules;

// Add services to the container.
var builder = WebApplication.CreateSlimBuilder(args);

// Register modules
IModule[] modules = [];
builder.Services.AddSingleton<IReadOnlyList<IModule>>(modules);

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services, builder.Configuration);
}

builder.Services.AddHealthChecks();

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

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
app.MapHealthChecks("/health");

app.MapGet("/api/modules", (IReadOnlyList<IModule> installedModules) =>
{
    return Results.Ok(installedModules.Select(m => new {
        m.Slug,
        m.DisplayName,
        m.Description,
        Kind = m.Kind.ToString(),
        Url = m.StaticFileUrlPrefix
    }));
});

foreach (var module in modules)
{
    module.MapEndpoints(app);
}

app.Run();
