using VM.Druckdienst.Services;

namespace VM.Druckdienst.Models;

public class Notenblatt
{
    public int Id { get; set; }
    public string Nummer { get; set; } = "";
    public string PdfPfad { get; set; } = "";
    public List<Seite> Seiten { get; set; } = new();
    public DuplexModus Duplex { get; set; } = DuplexModus.Nein;

    public Papierformat Format => Seiten.Count > 0 ? Seiten[0].Format : Papierformat.Unbekannt;

    public void SeitenLaden()
    {
        Seiten = PdfLeser.SeitenLesen(PdfPfad);
    }
}