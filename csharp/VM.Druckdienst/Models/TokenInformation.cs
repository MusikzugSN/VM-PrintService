namespace VM.Druckdienst.Models;

public class TokenInformation
{
    public string Token { get; set; } = "";
    public List<NotenEintrag> Noten { get; set; } = new();
}

public class NotenEintrag
{
    public int Id { get; set; }
    public int Nummer { get; set; }
}