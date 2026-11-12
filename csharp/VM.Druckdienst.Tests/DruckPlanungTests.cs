using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class DruckPlanungTests
{
    private static Seite SeiteMit(int nummer, Papierformat format)
    {
        var (breite, hoehe) = format switch
        {
            Papierformat.A4 => (210.0, 297.0),
            Papierformat.A3 => (297.0, 420.0),
            Papierformat.A5 => (148.0, 210.0),
            _ => (100.0, 100.0)
        };

        return new Seite { Nummer = nummer, BreiteMm = breite, HoeheMm = hoehe };
    }

    private static Notenblatt Blatt(int nummer, params Papierformat[] formate)
    {
        return new Notenblatt
        {
            Id = nummer * 10,
            Nummer = nummer.ToString(),
            Seiten = formate.Select((f, i) => SeiteMit(i + 1, f)).ToList()
        };
    }

    private static List<string> Text(Druckgruppe gruppe) =>
        gruppe.Seiten.Select(s => $"Note {s.Notenblatt.Nummer}, Seite {s.Seite.Nummer}").ToList();

    [Fact]
    public void Sortieren_NachNummer()
    {
        var noten = new List<Notenblatt> { Blatt(3, Papierformat.A4), Blatt(1, Papierformat.A4), Blatt(2, Papierformat.A4) };

        var sortiert = DruckPlanung.Sortieren(noten);

        Assert.Equal(new[] { "1", "2", "3" }, sortiert.Select(n => n.Nummer));
    }

    [Fact]
    public void Sortieren_LeereListe()
    {
        Assert.Empty(DruckPlanung.Sortieren(new List<Notenblatt>()));
    }

    [Fact]
    public void Gruppieren_WieToken_abc123()
    {
        // Same as the test API: 1 = A4, 2 = A5, 3 = A3, 4 = A4 + A3
        var noten = DruckPlanung.Sortieren(new List<Notenblatt>
        {
            Blatt(3, Papierformat.A3),
            Blatt(1, Papierformat.A4),
            Blatt(4, Papierformat.A4, Papierformat.A3),
            Blatt(2, Papierformat.A5)
        });

        var gruppen = DruckPlanung.Gruppieren(noten);

        Assert.Equal(new[] { Papierformat.A4, Papierformat.A3, Papierformat.A5 }, gruppen.Select(g => g.Format));
        Assert.Equal(new[] { "Note 1, Seite 1", "Note 4, Seite 1" }, Text(gruppen[0]));
        Assert.Equal(new[] { "Note 3, Seite 1", "Note 4, Seite 2" }, Text(gruppen[1]));
        Assert.Equal(new[] { "Note 2, Seite 1" }, Text(gruppen[2]));
    }

    [Fact]
    public void Gruppieren_ZweiSeitenGleichesFormat_BleibenZusammen()
    {
        var noten = new List<Notenblatt> { Blatt(1, Papierformat.A4, Papierformat.A4) };

        var gruppen = DruckPlanung.Gruppieren(noten);

        Assert.Single(gruppen);
        Assert.Equal(new[] { "Note 1, Seite 1", "Note 1, Seite 2" }, Text(gruppen[0]));
    }

    [Fact]
    public void Gruppieren_UnbekanntKommtAmEnde()
    {
        var noten = new List<Notenblatt> { Blatt(1, Papierformat.Unbekannt), Blatt(2, Papierformat.A4) };

        var gruppen = DruckPlanung.Gruppieren(noten);

        Assert.Equal(new[] { Papierformat.A4, Papierformat.Unbekannt }, gruppen.Select(g => g.Format));
    }

    [Fact]
    public void Gruppieren_LeereListe_KeineGruppen()
    {
        Assert.Empty(DruckPlanung.Gruppieren(new List<Notenblatt>()));
    }
}
