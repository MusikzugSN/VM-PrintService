using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class CupsAusgabe
{
    public static List<string> DruckerNamen(string ausgabe)
    {
        var namen = new List<string>();

        foreach (string zeile in ausgabe.Split('\n'))
        {
            if (zeile.StartsWith("printer "))
            {
                namen.Add(zeile.Split(' ')[1]);
            }
        }

        return namen;
    }

    public static string? StandardDrucker(string ausgabe)
    {
        ausgabe = ausgabe.Trim();
        int pos = ausgabe.IndexOf(": ");

        if (ausgabe.StartsWith("no system default") || pos < 0)
        {
            return null;
        }

        return ausgabe.Substring(pos + 2).Trim();
    }

    public static List<Papierfach> Papierfaecher(string ausgabe)
    {
        var faecher = new List<Papierfach>();

        foreach (string zeile in ausgabe.Split('\n'))
        {
            if (!zeile.StartsWith("InputSlot/"))
            {
                continue;
            }

            string werte = zeile.Substring(zeile.IndexOf(':') + 1).Trim();

            foreach (string wert in werte.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                faecher.Add(new Papierfach
                {
                    Name = wert.TrimStart('*'),
                    IstStandard = wert.StartsWith('*')
                });
            }
        }

        return faecher;
    }
}