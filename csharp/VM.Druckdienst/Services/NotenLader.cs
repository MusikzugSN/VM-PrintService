using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class NotenLader
{
    private readonly ApiClient _api;

    public NotenLader(ApiClient api)
    {
        _api = api;
    }

    public async Task<List<Notenblatt>> NotenLaden(string token)
    {
        TokenInformation? info = await _api.TokenInformationLesen(token);

        if (info == null || info.Noten.Count == 0)
        {
            throw new InvalidOperationException("Zu diesem Token gibt es keine Noten.");
        }

        var noten = new List<Notenblatt>();

        foreach (NotenEintrag eintrag in info.Noten)
        {
            string pfad = await _api.DateiLaden(eintrag.Id, token);

            noten.Add(new Notenblatt
            {
                Id = eintrag.Id,
                Nummer = eintrag.Nummer,
                PdfPfad = pfad
            });
        }

        return noten;
    }
    public static void TempDateienLoeschen(string token)
    {
        string ordner = ApiClient.TempOrdner(token);

        try
        {
            if (Directory.Exists(ordner))
            {
                Directory.Delete(ordner, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}