using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Endpoints;

public static class DruckerEndpoints
{
    public static void MapDruckerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/printers", (DruckerService service) =>
        {
            string? standard = service.StandardDruckerLesen();

            var drucker = service.DruckerNamenLesen()
                .Select(name => new Drucker
                {
                    Name = name,
                    IstStandard = name == standard,
                    Papierfaecher = service.PapierfaecherLesen(name)
                })
                .ToList();

            return Results.Ok(drucker);
        });
    }
}