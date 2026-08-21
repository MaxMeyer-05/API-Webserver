using Serilog;

// Add services to the container.
var builder = WebApplication.CreateSlimBuilder(args);

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
}

app.MapHealthChecks("/health");
app.Run();
