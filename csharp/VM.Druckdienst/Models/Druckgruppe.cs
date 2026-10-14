namespace VM.Druckdienst.Models;

public class DruckSeite
{
    public Notenblatt Notenblatt { get; set; } = null!;
    public Seite Seite { get; set; } = null!;
}

public class Druckgruppe
{
    public Papierformat Format { get; set; }
    public List<DruckSeite> Seiten { get; set; } = new();
}