namespace VM.Druckdienst.Models;

public class Drucker
{
    public string Name { get; set; } = "";
    public bool IstStandard { get; set; }
    public List<Papierfach> Papierfaecher { get; set; } = new();
}