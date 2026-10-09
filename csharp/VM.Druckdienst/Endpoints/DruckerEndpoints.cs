using VM.Druckdienst.Models;

namespace VM.Druckdienst.Endpoints;

public static class DruckerEndpoints
{
    public static void MapDruckerEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/printers", () =>
        {
            var drucker = new List<Drucker>
            {
                new Drucker { Name = "Buero-Drucker", IstStandard = true,
                              Papierfaecher = new List<string> { "Tray1", "Tray2" } },
                new Drucker { Name = "PDF", IstStandard = false,
                              Papierfaecher = new List<string>() }
            };

            return Results.Ok(drucker);
        });
    }
}