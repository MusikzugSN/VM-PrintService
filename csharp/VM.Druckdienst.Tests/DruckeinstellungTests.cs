using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class DruckeinstellungTests
{
    private static List<Druckeinstellung> Einstellungen() => new()
    {
        new Druckeinstellung { Id = 1, Seitenzahl = 1, Modus = Druckmodus.Genau, Duplex = DuplexModus.Nein,       Dateiformat = 4 },
        new Druckeinstellung { Id = 2, Seitenzahl = 1, Modus = Druckmodus.Mehr,  Duplex = DuplexModus.LangeSeite, Dateiformat = 4 },
        new Druckeinstellung { Id = 3, Seitenzahl = 1, Modus = Druckmodus.Mehr,  Duplex = DuplexModus.KurzeSeite, Dateiformat = 3 }
    };

    [Theory]
    [InlineData(Druckmodus.Genau, 2, 2, true)]
    [InlineData(Druckmodus.Genau, 2, 3, false)]
    [InlineData(Druckmodus.Mehr, 2, 3, true)]
    [InlineData(Druckmodus.Mehr, 2, 2, false)]
    [InlineData(Druckmodus.Weniger, 4, 3, true)]
    [InlineData(Druckmodus.Weniger, 4, 4, false)]
    public void PasstZu_Modus(Druckmodus modus, int seitenzahl, int seiten, bool erwartet)
    {
        var einstellung = new Druckeinstellung { Seitenzahl = seitenzahl, Modus = modus, Dateiformat = 4 };

        Assert.Equal(erwartet, einstellung.PasstZu(seiten, Papierformat.A4));
    }

    [Fact]
    public void PasstZu_AnderesFormat_Nein()
    {
        var a4 = new Druckeinstellung { Seitenzahl = 1, Modus = Druckmodus.Genau, Dateiformat = 4 };

        Assert.False(a4.PasstZu(1, Papierformat.A3));
    }

    [Theory]
    [InlineData(3, Papierformat.A3)]
    [InlineData(4, Papierformat.A4)]
    [InlineData(5, Papierformat.A5)]
    [InlineData(6, Papierformat.Unbekannt)]
    public void Dateiformat_ZuPapierformat(int dateiformat, Papierformat erwartet)
    {
        Assert.Equal(erwartet, new Druckeinstellung { Dateiformat = dateiformat }.Format);
    }

    [Theory]
    [InlineData(1, Papierformat.A4, DuplexModus.Nein)]
    [InlineData(2, Papierformat.A4, DuplexModus.LangeSeite)]
    [InlineData(5, Papierformat.A4, DuplexModus.LangeSeite)]
    [InlineData(2, Papierformat.A3, DuplexModus.KurzeSeite)]
    public void EinstellungFinden_NachSeitenUndFormat(int seiten, Papierformat format, DuplexModus erwartet)
    {
        Assert.Equal(erwartet, Druckauftrag.EinstellungFinden(Einstellungen(), seiten, format)?.Duplex);
    }

    [Theory]
    [InlineData(1, Papierformat.A3)]
    [InlineData(1, Papierformat.A5)]
    [InlineData(0, Papierformat.A4)]
    public void EinstellungFinden_KeinePasst_GibtNull(int seiten, Papierformat format)
    {
        Assert.Null(Druckauftrag.EinstellungFinden(Einstellungen(), seiten, format));
    }

    [Fact]
    public void EinstellungFinden_GenauGewinnt()
    {
        var einstellungen = new List<Druckeinstellung>
        {
            new() { Id = 1, Seitenzahl = 1, Modus = Druckmodus.Mehr,  Duplex = DuplexModus.LangeSeite, Dateiformat = 4 },
            new() { Id = 2, Seitenzahl = 2, Modus = Druckmodus.Genau, Duplex = DuplexModus.Nein,       Dateiformat = 4 }
        };

        Assert.Equal(2, Druckauftrag.EinstellungFinden(einstellungen, 2, Papierformat.A4)?.Id);
    }

    [Fact]
    public void EinstellungFinden_LeereListe_GibtNull()
    {
        Assert.Null(Druckauftrag.EinstellungFinden(new List<Druckeinstellung>(), 1, Papierformat.A4));
    }

    [Fact]
    public void Json_WieVmRestfull()
    {
        string json = """[{"printConfigId":7,"pageCount":2,"mode":1,"duplex":2,"fileFormat":3,"createdAt":"2026-04-01T10:00:00"}]""";

        var liste = System.Text.Json.JsonSerializer.Deserialize<List<Druckeinstellung>>(json,
            new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;

        Assert.Equal(7, liste[0].Id);
        Assert.Equal(2, liste[0].Seitenzahl);
        Assert.Equal(Druckmodus.Mehr, liste[0].Modus);
        Assert.Equal(DuplexModus.KurzeSeite, liste[0].Duplex);
        Assert.Equal(Papierformat.A3, liste[0].Format);
    }
}