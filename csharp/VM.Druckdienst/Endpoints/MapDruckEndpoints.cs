using System.Text.RegularExpressions;
using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Endpoints;

public static class DruckEndpoints
{
    public static void MapDruckEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/print/{token}",
            (string token, Druckanfrage? anfrage, DruckerService service, IConfiguration config) =>
            {
                var fehler = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(token))
                {
                    fehler["token"] = new[] { "Kein Token angegeben." };
                }

                string? drucker = anfrage?.Drucker;

                if (string.IsNullOrWhiteSpace(drucker))
                {
                    fehler["drucker"] = new[] { "Kein Drucker angegeben." };
                }
                else if (!Regex.IsMatch(drucker, "^[A-Za-z0-9_-]+$"))
                {
                    fehler["drucker"] = new[] { "Der Druckername enthält ungültige Zeichen." };
                }
                else if (!service.DruckerNamenLesen().Contains(drucker))
                {
                    fehler["drucker"] = new[] { $"Drucker '{drucker}' gibt es nicht." };
                }

                if (fehler.Count > 0)
                {
                    return Results.ValidationProblem(fehler);  
                }

                var fachZuordnung = new Dictionary<Papierformat, string>();
                foreach (var eintrag in config.GetSection("Papierfaecher").GetChildren())
                {
                    if (Enum.TryParse(eintrag.Key, out Papierformat format) && eintrag.Value != null)
                    {
                        fachZuordnung[format] = eintrag.Value;
                    }
                }

                string ordner = config["TestPdfOrdner"] ?? "";
                var noten = TestNoten.Laden(ordner);

                try
                {
                    var auftrag = new Druckauftrag(token, drucker!, noten, service, fachZuordnung);
                    Druckergebnis ergebnis = auftrag.Ausfuehren();
                    return Results.Ok(ergebnis);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Druckauftrag fehlgeschlagen",
                        detail: ex.Message,
                        statusCode: 500);
                }
            });
    }
}