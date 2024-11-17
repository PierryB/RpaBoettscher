using Application.Services;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;

string usuarioCatolica = string.Empty;
string senhaCatolica = string.Empty;
string pastaTemp = string.Empty;

#if RELEASE
    usuarioCatolica = args[0];
    senhaCatolica = args[1];
    pastaTemp = args[2];
#else
    usuarioCatolica = "teste";
    senhaCatolica = "teste";
    pastaTemp = $@"C:\temp\PdfCatolica\{Guid.NewGuid()}";
#endif

if (Directory.Exists(pastaTemp))
{
    Directory.Delete(pastaTemp, true);
}
Directory.CreateDirectory(pastaTemp);

StreamWriter log = new(pastaTemp + @"\log.txt");

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
    log.WriteLine("Execução iniciada.");
    log.WriteLine("------------------------------------------------------------");

    await new CatolicaService(log, usuarioCatolica, senhaCatolica, browser, pastaTemp).SiteCatolica(page);
}
catch (Exception ex)
{
    log.WriteLine(ex.Message);
    Console.Error.WriteLine(ex.Message);
    Console.WriteLine(ex.Message);
}
finally
{
    log.Dispose();
    await browser.CloseAsync();
}
