using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class CupsAusgabeTests
{

    [Fact]
    public void DruckerNamen_ZweiDrucker()
    {
        string ausgabe =
            "printer PDF is idle.  enabled since Tue Oct  6 11:00:47 2026\n" +
            "printer Buero_HP is idle.  enabled since Wed Oct  7 09:12:03 2026\n";

        Assert.Equal(new[] { "PDF", "Buero_HP" }, CupsAusgabe.DruckerNamen(ausgabe));
    }

    [Theory]
    [InlineData("")]
    [InlineData("lpstat: No destinations added.\n")]
    public void DruckerNamen_KeinDrucker(string ausgabe)
    {
        Assert.Empty(CupsAusgabe.DruckerNamen(ausgabe));
    }

    [Theory]
    [InlineData("system default destination: PDF\n", "PDF")]
    [InlineData("system default destination: Buero_HP", "Buero_HP")]
    public void StandardDrucker_Vorhanden(string ausgabe, string erwartet)
    {
        Assert.Equal(erwartet, CupsAusgabe.StandardDrucker(ausgabe));
    }

    [Theory]
    [InlineData("no system default destination\n")]
    [InlineData("")]
    public void StandardDrucker_Keiner(string ausgabe)
    {
        Assert.Null(CupsAusgabe.StandardDrucker(ausgabe));
    }

    [Fact]
    public void Papierfaecher_EchterDrucker()
    {
        string ausgabe =
            "PageSize/Media Size: Letter *A4 A3 A5\n" +
            "InputSlot/Media Source: *Auto Tray1 Tray2 Manual\n" +
            "Duplex/2-Sided Printing: *None DuplexNoTumble DuplexTumble\n";

        var faecher = CupsAusgabe.Papierfaecher(ausgabe);

        Assert.Equal(new[] { "Auto", "Tray1", "Tray2", "Manual" }, faecher.Select(f => f.Name));
        Assert.True(faecher[0].IstStandard);
        Assert.Single(faecher, f => f.IstStandard);
    }

    [Fact]
    public void Papierfaecher_StandardNichtAmAnfang()
    {
        var faecher = CupsAusgabe.Papierfaecher("InputSlot/Paper Source: Tray1 *Tray2\n");

        Assert.Equal("Tray2", faecher.Single(f => f.IstStandard).Name);
    }

    [Fact]
    public void Papierfaecher_PdfDruckerOhneFaecher()
    {
        string ausgabe =
            "PageSize/Media Size: Letter *A4 A3 A5\n" +
            "Resolution/Resolution: *600dpi\n";

        Assert.Empty(CupsAusgabe.Papierfaecher(ausgabe));
    }
}