using UglyToad.PdfPig;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class PdfLeser
{
    public static List<Seite> SeitenLesen(string pfad)
    {
        var seiten = new List<Seite>();

        using var dokument = PdfDocument.Open(pfad);

        foreach (var page in dokument.GetPages())
        {
            double breite = Seite.PunktZuMm(page.Width);
            double hoehe = Seite.PunktZuMm(page.Height);
            if (page.Rotation.Value == 90 || page.Rotation.Value == 270)
            {
                (breite, hoehe) = (hoehe, breite);
            }

            seiten.Add(new Seite { Nummer = page.Number, BreiteMm = breite, HoeheMm = hoehe });
        }

        return seiten;
    }

}