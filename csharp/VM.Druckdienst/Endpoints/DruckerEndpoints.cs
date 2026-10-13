using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Endpoints;

public static class DruckerEndpoints
{
    public static void MapDruckerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/printers", (DruckerService service) =>
        {
            try
            {
                string status = service.BefehlAusfuehren("lpstat -r");
                if (!status.Contains("scheduler is running"))
                {
                    return Results.Problem(
                        title: "CUPS läuft nicht",
                        detail: "Der Druckdienst von Linux (CUPS) ist nicht gestartet.",
                        statusCode: 503);
                }

                string? standard = service.StandardDruckerLesen();

                var drucker = service.DruckerNamenLesen()
                    .Select(name => new Drucker
                    {
                        Name = name,
                        IstStandard = name == standard,
                        Papierfaecher = service.PapierfaecherLesen(name)
                    })
                    .ToList();

                if (drucker.Count == 0)
                {
                    return Results.Problem(
                        title: "Kein Drucker gefunden",
                        detail: "In CUPS ist kein Drucker eingerichtet.",
                        statusCode: 404);
                }

                return Results.Ok(drucker);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Fehler beim Lesen der Drucker",
                    detail: ex.Message,
                    statusCode: 500);
            }
        });
    }
}