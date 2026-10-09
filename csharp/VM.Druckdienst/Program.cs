using VM.Druckdienst.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));
app.MapDruckerEndpoints();

app.Run();