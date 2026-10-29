var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();



string ordner = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["TestPdfOrdner"] ?? "../TestPdfs"));


var dateien = new Dictionary<int, string>
{
    [10] = "test-a4.pdf",
    [20] = "test-a5.pdf",
    [30] = "test-a3.pdf",
    [40] = "test-gemischt.pdf",
    [50] = "test-kaputt.pdf",     
    [60] = "test-letter.pdf"      
};


var tokens = new Dictionary<string, int[]>
{
    ["abc123"] = new[] { 10, 20, 30, 40 },  
    ["kaputt"] = new[] { 10, 99 },           
    ["pdf-kaputt"] = new[] { 10, 50 },                  
    ["letter"] = new[] { 10, 60 }            
};

app.MapGet("/api/v1/print/token-information", (string token) =>
{
    if (!tokens.TryGetValue(token, out int[]? ids))
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(new
    {
        token,
        noten = ids.Select((id, i) => new { id, nummer = i + 1 })
    });
});

app.MapGet("/api/v1/print/print-information", (string token) =>
{
    if (!tokens.ContainsKey(token))
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(new[]
    {
        new { id = 1, name = "Einseitig",    minSeiten = 1, maxSeiten = 1,   duplex = false },
        new { id = 2, name = "Doppelseitig", minSeiten = 2, maxSeiten = 999, duplex = true  }
    });
});

app.MapGet("/api/v1/print/file/{id:int}", (int id, string token) =>
{
    if (!tokens.ContainsKey(token))
        return Results.NotFound(new { fehler = "Token ungültig" });

    if (!dateien.ContainsKey(id))
        return Results.NotFound(new { fehler = $"Datei {id} gibt es nicht" });

    return Results.File(Path.Combine(ordner, dateien[id]), "application/pdf");
});

app.Run();