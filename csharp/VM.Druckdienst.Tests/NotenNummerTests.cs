using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class NotenNummerTests
{
    [Theory]
    [InlineData("2", "10")]
    [InlineData("10", "12")]
    [InlineData("12", "12a")]
    [InlineData("12a", "12b")]
    [InlineData("12b", "13")]
    [InlineData("99", "Marsch")]
    [InlineData(" 7", "8")]
    public void KleinerAls(string kleiner, string groesser)
    {
        Assert.True(NotenNummer.Vergleichen(kleiner, groesser) < 0);
        Assert.True(NotenNummer.Vergleichen(groesser, kleiner) > 0);
    }

    [Theory]
    [InlineData("12a", "12A")]
    [InlineData("5", "5")]
    [InlineData("", "")]
    public void Gleich(string a, string b)
    {
        Assert.Equal(0, NotenNummer.Vergleichen(a, b));
    }

    [Fact]
    public void Sortieren_WieImNotenordner()
    {
        var noten = new[] { "12a", "10", "2", "12", "Marsch", "1" }
            .Select(n => new Notenblatt { Nummer = n })
            .ToList();

        var sortiert = DruckPlanung.Sortieren(noten);

        Assert.Equal(new[] { "1", "2", "10", "12", "12a", "Marsch" }, sortiert.Select(n => n.Nummer));
    }
}
