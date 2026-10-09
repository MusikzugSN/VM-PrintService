namespace VM.Druckdienst.Models;

public class Drucker
{
    public string Name { get; set; } = "";
    public bool IstStandard { get; set; }
    public List<string> Papierfaecher { get; set; } = new();
}