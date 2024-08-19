using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using PuppeteerSharp;
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
            // Configuração do Puppeteer para usar o Chromium da camada
            var options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = "/opt/chrome/chrome",  // Caminho correto para o executável Chromium
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            };

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();

            // Navegação até a página e geração do PDF
            await page.GoToAsync("https://portal.catolicasc.org.br/FrameHTML/web/app/edu/PortalEducacional/login/");
            Console.WriteLine("Abriu navegador");
            string pdfBase64 = "Teste";

            return pdfBase64;
        }
    }
}
