using VM.Druckdienst.Services;

namespace VM.Druckdienst.Models;

public class Notenblatt
{
    public int Id { get; set; }          
    public int Nummer { get; set; }      
    public string PdfPfad { get; set; } = "";
    public List<Seite> Seiten { get; set; } = new();
    public bool Duplex { get; set; }


    public void SeitenLaden()
    {
        Seiten = PdfLeser.SeitenLesen(PdfPfad);
    }
}