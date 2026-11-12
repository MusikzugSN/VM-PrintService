namespace VM.Druckdienst.Services;

public static class NotenNummer
{
    public static readonly IComparer<string> Vergleich = Comparer<string>.Create(Vergleichen);

    public static int Vergleichen(string? a, string? b)
    {
        var (zahlA, restA) = Zerlegen(a);
        var (zahlB, restB) = Zerlegen(b);

        if (zahlA.HasValue && zahlB.HasValue && zahlA != zahlB)
        {
            return zahlA.Value.CompareTo(zahlB.Value);
        }

        if (zahlA.HasValue != zahlB.HasValue)
        {
            return zahlA.HasValue ? -1 : 1;
        }

        return string.Compare(restA, restB, StringComparison.OrdinalIgnoreCase);
    }

    private static (long? Zahl, string Text) Zerlegen(string? nummer)
    {
        nummer = (nummer ?? "").Trim();

        int laenge = 0;
        while (laenge < nummer.Length && laenge < 18 && char.IsAsciiDigit(nummer[laenge]))
        {
            laenge++;
        }

        if (laenge == 0)
        {
            return (null, nummer);
        }

        return (long.Parse(nummer[..laenge]), nummer[laenge..]);
    }
}