using VM.Druckdienst.Models;

namespace VM.Druckdienst.Services;

public static class TestNoten
{
    public static List<Notenblatt> Laden(string ordner)
    {
        return new List<Notenblatt>
        {
            new() { Id = 30, Nummer = 3, PdfPfad = Path.Combine(ordner, "test-a3.pdf") },
            new() { Id = 10, Nummer = 1, PdfPfad = Path.Combine(ordner, "test-a4.pdf") },
            new() { Id = 40, Nummer = 4, PdfPfad = Path.Combine(ordner, "test-gemischt.pdf") },
            new() { Id = 20, Nummer = 2, PdfPfad = Path.Combine(ordner, "test-a5.pdf") }
        };
    }
}