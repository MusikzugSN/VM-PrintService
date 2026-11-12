using System.Text.Json.Serialization;

namespace VM.Druckdienst.Models;

public enum Druckmodus
{
    Genau = 0,
    Mehr = 1,
    Weniger = 2
}

public enum DuplexModus
{
    Nein = 0,
    LangeSeite = 1,
    KurzeSeite = 2
}

public class Druckeinstellung
{
    [JsonPropertyName("printConfigId")]
    public int Id { get; set; }

    [JsonPropertyName("pageCount")]
    public int Seitenzahl { get; set; }

    [JsonPropertyName("mode")]
    public Druckmodus Modus { get; set; }

    [JsonPropertyName("duplex")]
    public DuplexModus Duplex { get; set; }

    [JsonPropertyName("fileFormat")]
    public int Dateiformat { get; set; }

    public Papierformat Format => Dateiformat switch
    {
        3 => Papierformat.A3,
        4 => Papierformat.A4,
        5 => Papierformat.A5,
        _ => Papierformat.Unbekannt
    };

    public bool PasstZu(int seiten, Papierformat format)
    {
        if (format != Format)
        {
            return false;
        }

        return Modus switch
        {
            Druckmodus.Genau => seiten == Seitenzahl,
            Druckmodus.Mehr => seiten > Seitenzahl,
            Druckmodus.Weniger => seiten < Seitenzahl,
            _ => false
        };
    }
}