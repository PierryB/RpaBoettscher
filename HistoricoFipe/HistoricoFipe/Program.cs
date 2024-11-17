using Application.Services;
using PuppeteerExtraSharp;
using PuppeteerSharp;

string mesReferencia = string.Empty;
string pastaTemp = string.Empty;

#if RELEASE
    mesReferencia = args[0];
    pastaTemp = args[1];
#else
    mesReferencia = "setembro/2024";
    pastaTemp = $@"C:\temp\HistoricoFipe\{Guid.NewGuid()}";
#endif

if (Directory.Exists(pastaTemp))
{
    Directory.Delete(pastaTemp, true);
}
Directory.CreateDirectory(pastaTemp);

StreamWriter log = new(pastaTemp + @"\log.txt");
StreamWriter csv = new(pastaTemp + @"\TabelaFipe.csv");
string csvPath = pastaTemp + @"\TabelaFipe.csv";
string msgExecucao = "INÍCIO";
var puppeteer = new PuppeteerExtra();
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
    log.WriteLine(msgExecucao);
    log.WriteLine("------------------------------------------------------------");

    await new HistoricoFipeService(log, csv, mesReferencia).SiteFipe(page);

    msgExecucao = "FIM";
}
catch (Exception ex)
{
    msgExecucao = $"Erro na execução: {ex.Message}";
}
finally
{
    await browser.CloseAsync();
    csv.Close();
    new ConverterCsvService(pastaTemp).ExportarParaExcel(csvPath);
    log.WriteLine("------------------------------------------------------------");
    log.WriteLine(msgExecucao);
    log.Dispose();
    csv.Dispose();
}