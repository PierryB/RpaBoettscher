using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.IO;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DownloadPdfCatolica
{
    public class LambdaHandler
    {
        public static async Task<APIGatewayProxyResponse> HandleRequest()
        {
            string pdfBase64 = await DownloadPdfCatolica();

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Teste deu certo!"
            };
        }

        private static async Task<string> DownloadPdfCatolica()
        {
            // Configuração do Puppeteer
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(); // Baixa a versão padrão do Chromium

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();

            // Navegação até a página e geração do PDF
            await page.GoToAsync("https://portal.catolicasc.org.br/FrameHTML/web/app/edu/PortalEducacional/login/");
            Console.WriteLine("Abriu navegador");
            /*var pdfStream = await page.PdfStreamAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });

            // Ler o PDF para um array de bytes
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream);
            byte[] pdfBytes = memoryStream.ToArray();

            // Converter para Base64
            string pdfBase64 = System.Convert.ToBase64String(pdfBytes);*/
            string pdfBase64 = "Teste";

            return pdfBase64;
        }
    }
}
