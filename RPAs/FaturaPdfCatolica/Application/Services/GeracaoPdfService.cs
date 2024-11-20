using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Application.Services;

public class GeracaoPdfService
{
    public byte[] GerarPDF(string pdf)
    {
        byte[] bytespdf = File.ReadAllBytes(pdf);
        MemoryStream ms = new(bytespdf);
        return ms.ToArray();
    }
}
