using System.Reflection;
using Serilog;
using Serilog.Events;
using VM.Druckdienst.Endpoints;
using VM.Druckdienst.Services;

var builder = WebApplication.CreateBuilder(args);

var konfigFehler = KonfigPruefung.Pruefen(builder.Configuration);

if (konfigFehler.Count > 0)
{
    Console.Error.WriteLine("Der Druckdienst startet nicht. Fehler in appsettings.json:");

    foreach (string f in konfigFehler)
    {
        Console.Error.WriteLine($"  - {f}");
    }

    return 1;
}

string logOrdner = builder.Configuration["LogOrdner"] ?? "logs";

if (!Path.IsPathRooted(logOrdner))
{
    logOrdner = Path.Combine(AppContext.BaseDirectory, logOrdner);
}

builder.Host.UseSerilog((context, logConfig) => logConfig
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(logOrdner, "druckdienst-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

string[] erlaubteUrspruenge = builder.Configuration
    .GetSection("ErlaubteUrspruenge")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Website", policy =>
        policy.WithOrigins(erlaubteUrspruenge)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<DruckerService>();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBasisUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue("ApiTimeoutSekunden", 30));
});
builder.Services.AddScoped<NotenLader>();

var app = builder.Build();

app.UseCors("Website");

string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "unbekannt";

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok", version }));
app.MapDruckerEndpoints();
app.MapDruckEndpoints();

app.Logger.LogInformation("VM-Druckdienst {Version} gestartet, Logs in {LogOrdner}", version, logOrdner);

app.Run();

return 0;