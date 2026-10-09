using VM.Druckdienst.Endpoints;
using VM.Druckdienst.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Website", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddSingleton<DruckerService>();
var app = builder.Build();

app.UseCors("Website");

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));
app.MapDruckerEndpoints();

app.Run();