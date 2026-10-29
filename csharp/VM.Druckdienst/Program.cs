using Serilog;
using Serilog.Events;
using VM.Druckdienst.Endpoints;
using VM.Druckdienst.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, logConfig) => logConfig
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "druckdienst-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Website", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<DruckerService>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBasisUrl"]!);
});
builder.Services.AddScoped<NotenLader>();

var app = builder.Build();

app.UseCors("Website");

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));
app.MapDruckerEndpoints();
app.MapDruckEndpoints();

app.Run();
