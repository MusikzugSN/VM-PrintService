using VM.Druckdienst.Models;
using VM.Druckdienst.Services;

namespace VM.Druckdienst.Tests;

public class SeitenOptionTests
{
    [Theory]
    [InlineData(DuplexModus.Nein, 2, " -o sides=one-sided")]
    [InlineData(DuplexModus.LangeSeite, 2, " -o sides=two-sided-long-edge")]
    [InlineData(DuplexModus.KurzeSeite, 3, " -o sides=two-sided-short-edge")]
    [InlineData(DuplexModus.LangeSeite, 1, " -o sides=one-sided")]
    public void SeitenOption(DuplexModus duplex, int seiten, string erwartet)
    {
        Assert.Equal(erwartet, DruckerService.SeitenOption(duplex, seiten));
    }
}
