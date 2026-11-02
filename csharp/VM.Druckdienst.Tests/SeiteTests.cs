using VM.Druckdienst.Models;

namespace VM.Druckdienst.Tests;

public class SeiteTests
{
    [Theory]
    [InlineData(210, 297, Papierformat.A4)]
    [InlineData(297, 210, Papierformat.A4)]
    [InlineData(297, 420, Papierformat.A3)]
    [InlineData(420, 297, Papierformat.A3)]
    [InlineData(148, 210, Papierformat.A5)]
    [InlineData(214, 300, Papierformat.A4)]
    [InlineData(205, 292, Papierformat.A4)]
    [InlineData(216, 297, Papierformat.Unbekannt)]
    [InlineData(216, 279, Papierformat.Unbekannt)]
    [InlineData(100, 100, Papierformat.Unbekannt)]
    public void BestimmeFormat(double breiteMm, double hoeheMm, Papierformat erwartet)
    {
        var seite = new Seite { BreiteMm = breiteMm, HoeheMm = hoeheMm };

        Assert.Equal(erwartet, seite.BestimmeFormat());
    }

    [Theory]
    [InlineData(595.28, 210.0)]
    [InlineData(841.89, 297.0)]
    [InlineData(1190.55, 420.0)]
    public void PunktZuMm(double punkt, double erwartetMm)
    {
        Assert.Equal(erwartetMm, Seite.PunktZuMm(punkt), 1);
    }

    [Theory]
    [InlineData(297, 210, true)]
    [InlineData(210, 297, false)]
    public void IstQuer(double breiteMm, double hoeheMm, bool erwartet)
    {
        var seite = new Seite { BreiteMm = breiteMm, HoeheMm = hoeheMm };

        Assert.Equal(erwartet, seite.IstQuer);
    }
}
