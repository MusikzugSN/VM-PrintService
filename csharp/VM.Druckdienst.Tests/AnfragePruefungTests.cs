using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class AnfragePruefungTests
{
    private static List<string> NurPdf() => new() { "PDF" };

    [Fact]
    public void AllesOk_KeineFehler()
    {
        Assert.Empty(AnfragePruefung.Pruefen("abc123", "PDF", NurPdf));
    }

    [Theory]
    [InlineData(null, "Kein Token angegeben.")]
    [InlineData("", "Kein Token angegeben.")]
    [InlineData("abc.123", "Der Token enthält ungültige Zeichen.")]
    [InlineData("../windows", "Der Token enthält ungültige Zeichen.")]
    [InlineData("abc 123", "Der Token enthält ungültige Zeichen.")]
    public void FalscherToken(string? token, string meldung)
    {
        var fehler = AnfragePruefung.Pruefen(token, "PDF", NurPdf);

        Assert.Single(fehler);
        Assert.Equal(meldung, fehler["token"][0]);
    }

    [Theory]
    [InlineData(null, "Kein Drucker angegeben.")]
    [InlineData("", "Kein Drucker angegeben.")]
    [InlineData("PDF; rm -rf /", "Der Druckername enthält ungültige Zeichen.")]
    [InlineData("GibtEsNicht", "Drucker 'GibtEsNicht' gibt es nicht.")]
    public void FalscherDrucker(string? drucker, string meldung)
    {
        var fehler = AnfragePruefung.Pruefen("abc123", drucker, NurPdf);

        Assert.Single(fehler);
        Assert.Equal(meldung, fehler["drucker"][0]);
    }

    [Fact]
    public void TokenUndDruckerFalsch_ZweiFehler()
    {
        var fehler = AnfragePruefung.Pruefen("abc.123", null, NurPdf);

        Assert.Equal(2, fehler.Count);
        Assert.True(fehler.ContainsKey("token"));
        Assert.True(fehler.ContainsKey("drucker"));
    }

    [Fact]
    public void UngueltigerName_DruckerlisteWirdNichtGelesen()
    {
        int aufrufe = 0;
        List<string> Zaehlen() { aufrufe++; return NurPdf(); }

        AnfragePruefung.Pruefen("abc123", "PDF; rm -rf /", Zaehlen);

        Assert.Equal(0, aufrufe); 
    }
}