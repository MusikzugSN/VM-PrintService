using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class Druckauftrag
{
    public static Druckeinstellung? EinstellungFinden(List<Druckeinstellung> einstellungen, int seiten)
    {
        return einstellungen.FirstOrDefault(e => e.PasstZu(seiten));
    }

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
            try
            {
                blatt.SeitenLaden();
            }
            catch (Exception ex)
            {
                throw new PdfNichtLesbarException(
                    $"Note {blatt.Nummer} (Datei {blatt.Id}) ist kein lesbares PDF.", ex);
            }
        }

        var sortiert = DruckPlanung.Sortieren(Noten);

        var gruppen = DruckPlanung.Gruppieren(sortiert);

        _druckerService.GruppenDrucken(gruppen, Drucker, _fachZuordnung, ergebnis);

        return ergebnis;
    }


}
