using Application.Services;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

string diretorioTemp = $@"C:\temp\PdfCatolica\{Guid.NewGuid()}";

if (Directory.Exists(diretorioTemp))
{
    Directory.Delete(diretorioTemp, true);
}
Directory.CreateDirectory(diretorioTemp);

StreamWriter log = new(diretorioTemp + @"\logCatolica.txt");
string usuarioCatolica = string.Empty;
string senhaCatolica = string.Empty;
StealthPlugin stealth = new();
var puppeteer = new PuppeteerExtra();
puppeteer.Use(stealth);
await new BrowserFetcher().DownloadAsync();

var browser = await puppeteer.LaunchAsync(new LaunchOptions()
{
    DumpIO = true,
    Headless = false,
    Args = ["--start-maximized"]
});
var page = await browser.NewPageAsync();
await page.SetViewportAsync(new ViewPortOptions()
{
    Width = 1920,
    Height = 1080,
});

try
{
#if RELEASE
    usuarioCatolica = args[0];
    senhaCatolica = args[1];
#else
    usuarioCatolica = "teste";
    senhaCatolica = "teste";
#endif
    log.WriteLine("Execução iniciada!");
    log.WriteLine("------------------------------------------------------------");

    /*string pdf = @"C:\temp\declinacoes.pdf";
    byte[] pdfBytesTeste = new GeracaoPdfService().GerarPDF(pdf);
    Console.WriteLine("teste");
    Console.WriteLine("teste");
    Console.WriteLine("teste");
    Console.OpenStandardOutput().Write(pdfBytesTeste, 0, pdfBytesTeste.Length);*/

    await new CatolicaService(log, usuarioCatolica, senhaCatolica, browser, diretorioTemp).SiteCatolica(page);
}
catch (Exception ex)
{
    log.WriteLine(ex.Message);
    Console.WriteLine(ex.Message);
}
finally
{
    log.Dispose();
    await browser.CloseAsync();
}
