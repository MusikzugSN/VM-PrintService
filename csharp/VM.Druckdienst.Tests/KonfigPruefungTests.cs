using Microsoft.Extensions.Configuration;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class KonfigPruefungTests
{
    private static IConfiguration Konfig(params (string Key, string? Wert)[] aenderungen)
    {
        var werte = new Dictionary<string, string?>
        {
            ["ApiBasisUrl"] = "http://localhost:8417/api/v1/",
            ["ApiTimeoutSekunden"] = "30",
            ["Papierfaecher:A4"] = "Tray1",
            ["Papierfaecher:A3"] = "Tray2"
        };

        foreach (var (key, wert) in aenderungen)
        {
            werte[key] = wert;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(werte).Build();
    }

    [Fact]
    public void KorrekteKonfig_KeineFehler()
    {
        Assert.Empty(KonfigPruefung.Pruefen(Konfig()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("localhost:8417/api/v1/")]
    [InlineData("ftp://localhost:8417/api/v1/")]
    [InlineData("http://localhost:8417/api/v1")]
    public void FalscheApiBasisUrl_EinFehler(string? url)
    {
        var fehler = KonfigPruefung.Pruefen(Konfig(("ApiBasisUrl", url)));

        Assert.Single(fehler);
        Assert.Contains("ApiBasisUrl", fehler[0]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("301")]
    [InlineData("abc")]
    public void FalscherTimeout_EinFehler(string timeout)
    {
        var fehler = KonfigPruefung.Pruefen(Konfig(("ApiTimeoutSekunden", timeout)));

        Assert.Single(fehler);
        Assert.Contains("ApiTimeoutSekunden", fehler[0]);
    }

    [Theory]
    [InlineData("Tray1; rm -rf /")]
    [InlineData("Tray 1")]
    [InlineData("")]
    public void FalschesFach_EinFehler(string fach)
    {
        var fehler = KonfigPruefung.Pruefen(Konfig(("Papierfaecher:A4", fach)));

        Assert.Single(fehler);
        Assert.Contains("A4", fehler[0]);
    }

    [Fact]
    public void UnbekanntesFormat_EinFehler()
    {
        var fehler = KonfigPruefung.Pruefen(Konfig(("Papierfaecher:A6", "Tray3")));

        Assert.Single(fehler);
        Assert.Contains("A6", fehler[0]);
    }
}