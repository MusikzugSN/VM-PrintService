namespace VM.Druckdienst.Models;

public class Seite
{
    public int Nummer { get; set; }
    public double BreiteMm { get; set; }
    public double HoeheMm { get; set; }

    public bool IstQuer => BreiteMm > HoeheMm;
    public Papierformat Format => BestimmeFormat();

    public static double PunktZuMm(double punkt) => punkt * 25.4 / 72;

    public Papierformat BestimmeFormat()
    {
        double kurz = Math.Min(BreiteMm, HoeheMm);
        double lang = Math.Max(BreiteMm, HoeheMm);

        if (Passt(kurz, lang, 210, 297)) return Papierformat.A4;
        if (Passt(kurz, lang, 297, 420)) return Papierformat.A3;
        if (Passt(kurz, lang, 148, 210)) return Papierformat.A5;

        return Papierformat.Unbekannt;
    }

    private static bool Passt(double kurz, double lang, double sollKurz, double sollLang)
    {
        return Math.Abs(kurz - sollKurz) <= 5 && Math.Abs(lang - sollLang) <= 5;
    }
}