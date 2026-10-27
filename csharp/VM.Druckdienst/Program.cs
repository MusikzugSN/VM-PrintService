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
app.MapGet("/api/v1/test-einstellung/{seiten:int}", async (int seiten, ApiClient api) =>
{
    var einstellungen = await api.DruckeinstellungenLesen("abc123");
    return Druckauftrag.EinstellungFinden(einstellungen, seiten)?.Name ?? "keine passt";
});
app.MapGet("/api/v1/test-download/{id:int}", async (int id, ApiClient api) =>
{
    string pfad = await api.DateiLaden(id, "abc123");
    var seiten = PdfLeser.SeitenLesen(pfad);
    return new { pfad, formate = seiten.Select(s => s.Format.ToString()) };
});
app.MapGet("/api/v1/test-noten/{token}", async (string token, ApiClient api) =>
{
    var lader = new NotenLader(api);
    var noten = await lader.NotenLaden(token);
    return noten.Select(n => new { n.Nummer, n.Id, n.PdfPfad });
});
