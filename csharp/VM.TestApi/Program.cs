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
    [60] = "test-letter.pdf",
    [70] = "test-a4-2seiten.pdf"
};

var tokens = new Dictionary<string, (int Id, string Nummer)[]>
{
    ["abc123"] = new[] { (10, "1"), (20, "2"), (30, "3"), (40, "4") },
    ["kaputt"] = new[] { (10, "1"), (99, "2") },
    ["pdf-kaputt"] = new[] { (10, "1"), (50, "2") },
    ["letter"] = new[] { (10, "1"), (60, "2") },
    ["duplex"] = new[] { (10, "1"), (70, "2") },
    ["nummern"] = new[] { (10, "12a"), (20, "10"), (30, "2"), (70, "12") }
};

var gross = new List<(int, string)>();
for (int i = 0; i < 375; i++)
{
    dateien[1000 + i] = (i % 3) switch
    {
        0 => "test-a4.pdf",
        1 => "test-a3.pdf",
        _ => "test-gemischt.pdf"
    };
    gross.Add((1000 + i, (i + 1).ToString()));
}
tokens["gross"] = gross.ToArray();

var einstellungen = new[]
{
    new { printConfigId = 1, pageCount = 1, mode = 0, duplex = 0, fileFormat = 4 },
    new { printConfigId = 2, pageCount = 1, mode = 1, duplex = 1, fileFormat = 4 },
    new { printConfigId = 3, pageCount = 1, mode = 1, duplex = 2, fileFormat = 3 }
};

app.MapGet("/api/v1/print/token-information", (string token) =>
{
    if (token == "altformat")
        return Results.Ok(new { token, noten = new[] { new { id = 10, nummer = 1 } } });

    if (!tokens.TryGetValue(token, out var noten))
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(new
    {
        token,
        noten = noten.Select(n => new { id = n.Id, nummer = n.Nummer })
    });
});

app.MapGet("/api/v1/print/print-information", (string token) =>
{
    if (!tokens.ContainsKey(token))
        return Results.NotFound(new { fehler = "Token ungültig" });

    return Results.Ok(einstellungen);
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
