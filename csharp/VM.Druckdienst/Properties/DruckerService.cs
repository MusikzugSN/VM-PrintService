using System.Diagnostics;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class DruckerService
{
    // Runs a Linux command and returns its output.
    // On Windows (development) it goes through WSL, on Linux it runs directly.
    // LANG=C = English output, so we can read it the same way everywhere.
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

    // "lpstat -p" → line: "printer PDF is idle.  enabled since ..."
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
}