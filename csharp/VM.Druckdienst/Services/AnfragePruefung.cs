using System.Text.RegularExpressions;

namespace VM.Druckdienst.Services;

public static class AnfragePruefung
{
    private static readonly Regex ErlaubteZeichen = new("^[A-Za-z0-9_-]+$");

    public static Dictionary<string, string[]> Pruefen(string? token, string? drucker,
                                                       Func<List<string>> druckerLesen)
    {
        var fehler = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(token))
        {
            fehler["token"] = new[] { "Kein Token angegeben." };
        }
        else if (!ErlaubteZeichen.IsMatch(token))
        {
            fehler["token"] = new[] { "Der Token enthält ungültige Zeichen." };
        }

        if (string.IsNullOrWhiteSpace(drucker))
        {
            fehler["drucker"] = new[] { "Kein Drucker angegeben." };
        }
        else if (!ErlaubteZeichen.IsMatch(drucker))
        {
            fehler["drucker"] = new[] { "Der Druckername enthält ungültige Zeichen." };
        }
        else if (!druckerLesen().Contains(drucker))
        {
            fehler["drucker"] = new[] { $"Drucker '{drucker}' gibt es nicht." };
        }

        return fehler;
    }
}