using System.Net.Http.Json;
using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<TokenInformation?> TokenInformationLesen(string token)
    {
        return await _http.GetFromJsonAsync<TokenInformation>(
            $"print/token-information?token={Uri.EscapeDataString(token)}");
    }

    public async Task<List<Druckeinstellung>> DruckeinstellungenLesen(string token)
    {
        var liste = await _http.GetFromJsonAsync<List<Druckeinstellung>>(
            $"print/print-information?token={Uri.EscapeDataString(token)}");

        return liste ?? new List<Druckeinstellung>();
    }
    public async Task<string> DateiLaden(int id, string token)
    {
        string ordner = TempOrdner(token); Directory.CreateDirectory(ordner);

        byte[] daten = await _http.GetByteArrayAsync(
            $"print/file/{id}?token={Uri.EscapeDataString(token)}");

        string pfad = Path.Combine(ordner, $"{id}.pdf");
        await File.WriteAllBytesAsync(pfad, daten);

        return pfad;
    }
    public static string TempOrdner(string token)
    {
        return Path.Combine(Path.GetTempPath(), "vm-druckdienst", token);
    }

}