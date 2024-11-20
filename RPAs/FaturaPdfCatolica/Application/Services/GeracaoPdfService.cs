namespace Application.Services;

public class GeracaoPdfService
{
    public static byte[] GerarPDF(string pdf)
    {
        byte[] bytespdf = File.ReadAllBytes(pdf);
        MemoryStream ms = new(bytespdf);
        return ms.ToArray();
    }
}
