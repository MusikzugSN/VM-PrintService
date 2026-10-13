using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class DruckPlanung
{
    public static List<Notenblatt> Sortieren(List<Notenblatt> noten)
    {
        return noten.OrderBy(n => n.Nummer).ToList();
    }
}