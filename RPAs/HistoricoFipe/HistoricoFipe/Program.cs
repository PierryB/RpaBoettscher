﻿using Application.Services;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using static System.Net.WebRequestMethods;

string mesReferencia = string.Empty;
string pastaTemp = string.Empty;

#if RELEASE
    mesReferencia = args[0];
    pastaTemp = args[1];
#else
    mesReferencia = "10/2024";
    pastaTemp = $@"C:\temp\HistoricoFipe\{Guid.NewGuid()}";
#endif

if (Directory.Exists(pastaTemp))
{
    Directory.Delete(pastaTemp, true);
}
Directory.CreateDirectory(pastaTemp);

StreamWriter log = new(pastaTemp + @"\log.txt");
StreamWriter csv = new(pastaTemp + @"\TabelaFipe.csv");
string url = "https://veiculos.fipe.org.br/";
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
    await log.WriteLineAsync(msgExecucao);
    await log.WriteLineAsync("------------------------------------------------------------");

    await new HistoricoFipeService(log, csv, mesReferencia, url).SiteFipe(page);

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
    await log.WriteLineAsync("------------------------------------------------------------");
    await log.WriteLineAsync(msgExecucao);
    await log.DisposeAsync();
    await csv.DisposeAsync();
}