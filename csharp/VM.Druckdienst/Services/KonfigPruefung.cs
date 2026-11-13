using System.Text.RegularExpressions;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class KonfigPruefung
{
    public static List<string> Pruefen(IConfiguration config)
    {
        var fehler = new List<string>();

        string? url = config["ApiBasisUrl"];

        if (string.IsNullOrWhiteSpace(url))
        {
            fehler.Add("ApiBasisUrl fehlt.");
        }
        else if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                 (uri.Scheme != "http" && uri.Scheme != "https"))
        {
            fehler.Add($"ApiBasisUrl '{url}' ist keine gültige http/https-Adresse.");
        }
        else if (!url.EndsWith("/"))
        {
            fehler.Add($"ApiBasisUrl '{url}' muss mit '/' enden.");
        }

        string? timeout = config["ApiTimeoutSekunden"];

        if (timeout != null && (!int.TryParse(timeout, out int sekunden) || sekunden < 1 || sekunden > 300))
        {
            fehler.Add($"ApiTimeoutSekunden '{timeout}' muss eine Zahl zwischen 1 und 300 sein.");
        }

        foreach (var eintrag in config.GetSection("ErlaubteUrspruenge").GetChildren())
        {
            string? ursprung = eintrag.Value;

            if (string.IsNullOrWhiteSpace(ursprung) ||
                !Uri.TryCreate(ursprung, UriKind.Absolute, out Uri? u) ||
                (u.Scheme != "http" && u.Scheme != "https") ||
                u.AbsolutePath != "/" || ursprung.EndsWith("/"))
            {
                fehler.Add($"ErlaubteUrspruenge: '{ursprung}' muss eine Adresse ohne Pfad sein, z. B. http://localhost:4200");
            }
        }

        foreach (var eintrag in config.GetSection("Papierfaecher").GetChildren())
        {
            if (!Enum.TryParse(eintrag.Key, out Papierformat format) || format == Papierformat.Unbekannt)
            {
                fehler.Add($"Papierfaecher: '{eintrag.Key}' ist kein bekanntes Format (A4, A3, A5).");
            }
            else if (string.IsNullOrWhiteSpace(eintrag.Value) ||
                     !Regex.IsMatch(eintrag.Value, "^[A-Za-z0-9_-]+$"))
            {
                fehler.Add($"Papierfaecher: Fach für {eintrag.Key} ist leer oder enthält ungültige Zeichen.");

            }
        }

        return fehler;
    }
}
