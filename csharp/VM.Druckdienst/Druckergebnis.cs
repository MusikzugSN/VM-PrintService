namespace VM.Druckdienst.Models;

public class Druckergebnis
{
    public int Gesamt { get; set; }
    public int Erfolgreich { get; set; }
    public int Fehlgeschlagen { get; set; }
    public List<string> Fehler { get; set; } = new();

    public bool AllesGedruckt => Fehlgeschlagen == 0;
}