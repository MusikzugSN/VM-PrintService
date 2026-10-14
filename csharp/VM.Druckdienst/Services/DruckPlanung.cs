using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class DruckPlanung
{
    public static List<Notenblatt> Sortieren(List<Notenblatt> noten)
    {
        return noten.OrderBy(n => n.Nummer).ToList();
    }
    public static List<Druckgruppe> Gruppieren(List<Notenblatt> sortierteNoten)
    {
        var gruppen = new List<Druckgruppe>();

        foreach (var blatt in sortierteNoten)
        {
            foreach (var seite in blatt.Seiten)
            {
                var gruppe = gruppen.FirstOrDefault(g => g.Format == seite.Format);

                if (gruppe == null)
                {
                    gruppe = new Druckgruppe { Format = seite.Format };
                    gruppen.Add(gruppe);
                }

                gruppe.Seiten.Add(new DruckSeite { Notenblatt = blatt, Seite = seite });
            }
        }

        return gruppen.OrderBy(g => g.Format).ToList();
    }
}