namespace VM.Druckdienst.Models;

public class Druckeinstellung
{
    public bool PasstZu(int seiten)
    {
        return seiten >= MinSeiten && seiten <= MaxSeiten;
    }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int MinSeiten { get; set; }
    public int MaxSeiten { get; set; }
    public bool Duplex { get; set; }

}
