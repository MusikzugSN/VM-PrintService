using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class Druckauftrag
{
    public string Token { get; }
    public string Drucker { get; }
    public List<Notenblatt> Noten { get; }

    private readonly DruckerService _druckerService;
    private readonly Dictionary<Papierformat, string> _fachZuordnung;

    public Druckauftrag(string token, string drucker, List<Notenblatt> noten,
                        DruckerService druckerService, Dictionary<Papierformat, string> fachZuordnung)
    {
        Token = token;
        Drucker = drucker;
        Noten = noten;
        _druckerService = druckerService;
        _fachZuordnung = fachZuordnung;
    }

    public Druckergebnis Ausfuehren()
    {
        var ergebnis = new Druckergebnis();

        foreach (var blatt in Noten)
        {
            blatt.SeitenLaden();
        }

        var sortiert = DruckPlanung.Sortieren(Noten);

        var gruppen = DruckPlanung.Gruppieren(sortiert);

        ergebnis.Gesamt = gruppen.Sum(g => g.Seiten.Count);

        return ergebnis;
    }
}