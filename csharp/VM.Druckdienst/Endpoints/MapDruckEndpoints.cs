using System.Net;
using System.Text.RegularExpressions;
using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Endpoints;

public static class DruckEndpoints
{
    public static void MapDruckEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/print/{token}",
            async (string token, Druckanfrage? anfrage, DruckerService service,
                   NotenLader lader, IConfiguration config, ILoggerFactory logFactory) =>
            {
                var log = logFactory.CreateLogger("Druckauftrag");


                var fehler = new Dictionary<string, string[]>();

                if (string.IsNullOrWhiteSpace(token))
                {
                    fehler["token"] = new[] { "Kein Token angegeben." };
                }
                else if (!Regex.IsMatch(token, "^[A-Za-z0-9_-]+$"))
                {
                    fehler["token"] = new[] { "Der Token enthält ungültige Zeichen." };
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
                    log.LogWarning("Anfrage abgelehnt: Token {Token}, Drucker {Drucker}", token, drucker);
                    return Results.ValidationProblem(fehler);
                }

                log.LogInformation("Druckauftrag gestartet: Token {Token}, Drucker {Drucker}", token, drucker);

                var fachZuordnung = new Dictionary<Papierformat, string>();
                foreach (var eintrag in config.GetSection("Papierfaecher").GetChildren())
                {
                    if (Enum.TryParse(eintrag.Key, out Papierformat format) && eintrag.Value != null)
                    {
                        fachZuordnung[format] = eintrag.Value;
                    }
                }

                try
                {
                    List<Notenblatt> noten;

                    try
                    {
                        noten = await lader.NotenLaden(token);
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        log.LogWarning("Token {Token}: Token ungültig oder Datei fehlt, nichts gedruckt", token);
                        return Results.Problem(
                            title: "Token ungültig oder Datei fehlt",
                            detail: "Die API kennt diesen Token nicht, oder eine Notendatei fehlt. Es wurde nichts gedruckt.",
                            statusCode: 404);
                    }
                    catch (HttpRequestException ex)
                    {
                        log.LogError(ex, "Token {Token}: API nicht erreichbar", token);
                        return Results.Problem(
                            title: "API nicht erreichbar",
                            detail: ex.Message,
                            statusCode: 502);
                    }
                    catch (TaskCanceledException)
                    {
                        log.LogError("Token {Token}: API hat zu lange nicht geantwortet", token);
                        return Results.Problem(
                            title: "API nicht erreichbar",
                            detail: "Die API hat zu lange nicht geantwortet.",
                            statusCode: 502);
                    }
                    catch (InvalidOperationException ex)
                    {
                        log.LogWarning("Token {Token}: {Fehler}", token, ex.Message);
                        return Results.Problem(
                            title: "Keine Noten zum Token",
                            detail: ex.Message,
                            statusCode: 400);
                    }

                    log.LogInformation("Token {Token}: {Anzahl} Notenblätter geladen", token, noten.Count);

                    var auftrag = new Druckauftrag(token, drucker!, noten, service, fachZuordnung);
                    Druckergebnis ergebnis = auftrag.Ausfuehren();

                    log.LogInformation(
                        "Token {Token}: fertig, {Erfolgreich} von {Gesamt} Seiten gedruckt, {Fehlgeschlagen} fehlgeschlagen",
                        token, ergebnis.Erfolgreich, ergebnis.Gesamt, ergebnis.Fehlgeschlagen);

                    foreach (string f in ergebnis.Fehler)
                    {
                        log.LogWarning("Token {Token}: {Fehler}", token, f);
                    }

                    return Results.Ok(ergebnis);
                }
                catch (PdfNichtLesbarException ex)
                {
                    log.LogWarning("Token {Token}: {Fehler} Nichts gedruckt.", token, ex.Message);
                    return Results.Problem(
                        title: "PDF nicht lesbar",
                        detail: ex.Message + " Es wurde nichts gedruckt.",
                        statusCode: 422);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Token {Token}: Druckauftrag fehlgeschlagen", token);
                    return Results.Problem(
                        title: "Druckauftrag fehlgeschlagen",
                        detail: ex.Message,
                        statusCode: 500);
                }
                finally
                {
                    NotenLader.TempDateienLoeschen(token);
                }
            });
    }
}
