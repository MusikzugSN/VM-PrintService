using System.Diagnostics;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class DruckerService
{
    private readonly ILogger<DruckerService> _log;

    public DruckerService(ILogger<DruckerService> log)
    {
        _log = log;
    }


    public string BefehlAusfuehren(string befehl)
    {
        var info = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "wsl" : "env",
            Arguments = OperatingSystem.IsWindows() ? $"env LANG=C {befehl}" : $"LANG=C {befehl}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var prozess = Process.Start(info)!;
        string ausgabe = prozess.StandardOutput.ReadToEnd();
        prozess.WaitForExit();
        return ausgabe;
    }

   
    public List<string> DruckerNamenLesen()
    {
        string ausgabe = BefehlAusfuehren("lpstat -p");
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
    public string? StandardDruckerLesen()
    {
        string ausgabe = BefehlAusfuehren("lpstat -d").Trim();
        int pos = ausgabe.IndexOf(": ");

        if (ausgabe.StartsWith("no system default") || pos < 0)
        {
            return null;
        }

        return ausgabe.Substring(pos + 2).Trim();
    }
    public List<Papierfach> PapierfaecherLesen(string drucker)
    {
        string ausgabe = BefehlAusfuehren($"lpoptions -p {drucker} -l");
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
    public static string LinuxPfad(string pfad)
    {
        if (!OperatingSystem.IsWindows())
        {
            return pfad;
        }

        char laufwerk = char.ToLower(pfad[0]);
        return $"/mnt/{laufwerk}{pfad.Substring(2).Replace('\\', '/')}";
    }

    public void GruppenDrucken(List<Druckgruppe> gruppen, string drucker,
                               Dictionary<Papierformat, string> fachZuordnung, Druckergebnis ergebnis)
    {
        {
            foreach (var gruppe in gruppen)
            {
                var bloecke = new List<List<DruckSeite>>();

                foreach (var druckSeite in gruppe.Seiten)
                {
                    var letzter = bloecke.LastOrDefault();

                    if (letzter != null && letzter[0].Notenblatt == druckSeite.Notenblatt)
                    {
                        letzter.Add(druckSeite);
                    }
                    else
                    {
                        bloecke.Add(new List<DruckSeite> { druckSeite });
                    }
                }

                foreach (var block in bloecke)
                {
                    Notenblatt blatt = block[0].Notenblatt;
                    string seiten = string.Join(",", block.Select(s => s.Seite.Nummer));
                    string wo = $"Note {blatt.Nummer}, Seite {seiten}";

                    ergebnis.Gesamt += block.Count;

                    if (gruppe.Format == Papierformat.Unbekannt)
                    {
                        ergebnis.Fehlgeschlagen += block.Count;
                        ergebnis.Fehler.Add($"{wo}: unbekanntes Papierformat");
                        continue;
                    }

                    string befehl = $"lp -d {drucker} -o media={gruppe.Format} -o page-ranges={seiten}";

                    if (fachZuordnung.TryGetValue(gruppe.Format, out string? fach))
                    {
                        befehl += $" -o InputSlot={fach}";
                    }

                    befehl += blatt.Duplex && block.Count > 1
                        ? " -o sides=two-sided-long-edge"
                        : " -o sides=one-sided";

                    befehl += $" \"{LinuxPfad(blatt.PdfPfad)}\"";

                    _log.LogInformation("lp: {Befehl}", befehl);
                    string ausgabe = BefehlAusfuehren(befehl);

                    if (ausgabe.StartsWith("request id is"))
                    {
                        ergebnis.Erfolgreich += block.Count;
                    }
                    else
                    {
                        ergebnis.Fehlgeschlagen += block.Count;
                        ergebnis.Fehler.Add($"{wo}: lp hat nicht gedruckt");
                    }
                }
            }
        }
    }

}