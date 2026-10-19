var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string GueltigerToken = "abc123";
string ordner = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["TestPdfOrdner"] ?? "../TestPdfs"));

var noten = new Dictionary<int, (int Nummer, string Datei)>
{
    [10] = (1, "test-a4.pdf"),
    [20] = (2, "test-a5.pdf"),
    [30] = (3, "test-a3.pdf"),
    [40] = (4, "test-gemischt.pdf")
};

app.MapGet("/api/v1/print/token-information", (string token) =>
{
    if (token != GueltigerToken)
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(new
    {
        token,
        noten = noten.Select(n => new { id = n.Key, nummer = n.Value.Nummer })
    });
});

app.MapGet("/api/v1/print/print-information", (string token) =>
{
    if (token != GueltigerToken)
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(new[]
    {
        new { id = 1, name = "Einseitig",    minSeiten = 1, maxSeiten = 1,   duplex = false },
        new { id = 2, name = "Doppelseitig", minSeiten = 2, maxSeiten = 999, duplex = true  }
    });
});

app.MapGet("/api/v1/print/file/{id:int}", (int id, string token) =>
{
    if (token != GueltigerToken)
        return Results.NotFound(new { fehler = "Token ungültig" });

    if (!noten.ContainsKey(id))
        return Results.NotFound(new { fehler = $"Datei {id} gibt es nicht" });

    string pfad = Path.Combine(ordner, noten[id].Datei);
    return Results.File(pfad, "application/pdf");
});

app.Run();