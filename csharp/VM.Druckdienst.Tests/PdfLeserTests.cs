using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class PdfLeserTests
{
    private static string Pfad(string datei) =>
        Path.Combine(AppContext.BaseDirectory, "TestPdfs", datei);

    [Theory]
    [InlineData("test-a4.pdf", Papierformat.A4)]
    [InlineData("test-a3.pdf", Papierformat.A3)]
    [InlineData("test-a5.pdf", Papierformat.A5)]
    [InlineData("test-letter.pdf", Papierformat.Unbekannt)]
    public void EineSeite_RichtigesFormat(string datei, Papierformat erwartet)
    {
        List<Seite> seiten = PdfLeser.SeitenLesen(Pfad(datei));

        Assert.Single(seiten);
        Assert.Equal(erwartet, seiten[0].Format);
    }

    [Fact]
    public void Gemischt_A4_dann_A3()
    {
        List<Seite> seiten = PdfLeser.SeitenLesen(Pfad("test-gemischt.pdf"));

        Assert.Equal(2, seiten.Count);
        Assert.Equal(Papierformat.A4, seiten[0].Format);
        Assert.Equal(Papierformat.A3, seiten[1].Format);
    }

    [Fact]
    public void ZweiSeitenA4()
    {
        List<Seite> seiten = PdfLeser.SeitenLesen(Pfad("test-a4-2seiten.pdf"));

        Assert.Equal(2, seiten.Count);
        Assert.All(seiten, s => Assert.Equal(Papierformat.A4, s.Format));
    }

    [Fact]
    public void KaputtesPdf_WirftFehler()
    {
        Assert.ThrowsAny<Exception>(() => PdfLeser.SeitenLesen(Pfad("test-kaputt.pdf")));
    }
}