using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;
using PuppeteerSharp;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DownloadPdfCatolica
{
    public class LambdaHandler
    {
        public static async Task<APIGatewayProxyResponse> HandleRequest()
        {
            /*string[] files = Directory.GetFiles("/opt", "*", SearchOption.AllDirectories);
            string fileList = string.Join("\n", files);*/

            string pdfBase64 = await DownloadPdfCatolica();

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = pdfBase64
            };
        }

        private static async Task<string> DownloadPdfCatolica()
        {
            // Caminho para o arquivo compactado do Chromium
            string brotliFile = "/opt/nodejs/node_modules/@sparticuz/chromium/bin/al2.tar.br"; // ou al2023.tar.br
            string tarFile = "/tmp/chromium.tar";
            string outputDir = "/tmp/chromium"; // Diretório de extração

            // Descompactar o arquivo .br usando BrotliStream
            using (var brotliStream = new BrotliStream(File.OpenRead(brotliFile), CompressionMode.Decompress))
            using (var fileStream = File.Create(tarFile))
            {
                await brotliStream.CopyToAsync(fileStream);
            }

            // Extrair o arquivo .tar
            Directory.CreateDirectory(outputDir);
            System.Diagnostics.Process.Start("tar", $"-xf {tarFile} -C {outputDir}").WaitForExit();

            // Caminho para o executável do Chromium após descompactação
            string chromePath = Path.Combine(outputDir, "chrome");

            var options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = chromePath,
                Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
            };

            using var browser = await Puppeteer.LaunchAsync(options);
            using var page = await browser.NewPageAsync();

            await page.GoToAsync("https://portal.catolicasc.org.br/FrameHTML/web/app/edu/PortalEducacional/login/");
            Console.WriteLine("Abriu navegador");

            // Geração do PDF e retorno em Base64
            string pdfBase64 = "Teste"; // Substitua com sua lógica

            return pdfBase64;
        }
    }
}
