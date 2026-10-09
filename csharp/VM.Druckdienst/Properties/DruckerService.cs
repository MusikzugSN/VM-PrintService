using System.Diagnostics;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class DruckerService
{
   
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

}