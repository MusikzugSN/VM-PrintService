using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class DruckeinstellungTests
{
    private static List<Druckeinstellung> Einstellungen() => new()
    {
        new Druckeinstellung { Id = 1, Name = "Einseitig",    MinSeiten = 1, MaxSeiten = 1,   Duplex = false },
        new Druckeinstellung { Id = 2, Name = "Doppelseitig", MinSeiten = 2, MaxSeiten = 999, Duplex = true  }
    };

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void PasstZu_Einseitig(int seiten, bool erwartet)
    {
        Assert.Equal(erwartet, Einstellungen()[0].PasstZu(seiten));
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(999, true)]
    [InlineData(1000, false)]
    public void PasstZu_Doppelseitig(int seiten, bool erwartet)
    {
        Assert.Equal(erwartet, Einstellungen()[1].PasstZu(seiten));
    }

    [Theory]
    [InlineData(1, "Einseitig")]
    [InlineData(2, "Doppelseitig")]
    [InlineData(50, "Doppelseitig")]
    public void EinstellungFinden_NachSeitenzahl(int seiten, string erwartet)
    {
        Assert.Equal(erwartet, Druckauftrag.EinstellungFinden(Einstellungen(), seiten)?.Name);
    }

    [Fact]
    public void EinstellungFinden_KeinePasst_GibtNull()
    {
        Assert.Null(Druckauftrag.EinstellungFinden(Einstellungen(), 0));
    }

    [Fact]
    public void EinstellungFinden_LeereListe_GibtNull()
    {
        Assert.Null(Druckauftrag.EinstellungFinden(new List<Druckeinstellung>(), 1));
    }
}