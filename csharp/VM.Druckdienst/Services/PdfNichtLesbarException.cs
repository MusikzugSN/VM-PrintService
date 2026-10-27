namespace VM.Druckdienst.Services;

public class PdfNichtLesbarException : Exception
{
    public PdfNichtLesbarException(string message, Exception inner)
        : base(message, inner)
    {
    }
}