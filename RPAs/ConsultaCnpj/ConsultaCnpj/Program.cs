using Application.Services;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using System.Data;

string arquivoExcelCnpjs = string.Empty;
string pastaTemp = string.Empty;

#if RELEASE
    arquivoExcelCnpjs = args[0];
    pastaTemp = args[1];
#else
    arquivoExcelCnpjs = @"C:\temp\PlanilhaCnpjs.xlsx";
    pastaTemp = $@"C:\temp\rpa\{Guid.NewGuid()}";
#endif

if (String.IsNullOrEmpty(arquivoExcelCnpjs))
    throw new FileNotFoundException("Nenhum arquivo de CNPJs foi informado.");
else if (!File.Exists(arquivoExcelCnpjs))
    throw new FileNotFoundException("O arquivo de CNPJs informado não existe.");
else if (String.IsNullOrEmpty(pastaTemp))
    throw new FileNotFoundException("Nenhuma pasta temporária foi informada.");

Directory.CreateDirectory(pastaTemp);
if (!Directory.Exists(pastaTemp))
    throw new FileNotFoundException("A pasta temporária informada não existe.");

StreamWriter log = new(pastaTemp + @"\log.txt");
StreamWriter csv = new(pastaTemp + @"\TabelaCnpj.csv");
string url = "https://solucoes.receita.fazenda.gov.br/servicos/cnpjreva/cnpjreva_solicitacao.asp";
string csvPath = pastaTemp + @"\TabelaCnpj.csv";
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
    Width = 1024,
    Height = 768,
});

try
{
    await log.WriteLineAsync(msgExecucao);
    await log.WriteLineAsync("------------------------------------------------------------");

    var listaCnpjs = new LerPlanilhaCnpjsService(arquivoExcelCnpjs).ObterListaDeCnpjs();
    if (listaCnpjs.Count == 0)
        throw new Exception("Não havia nenhum CNPJ para consulta no Excel informado.");

    var tabelaCnpjs = await new ConsultaCnpjService(log, csv, url, listaCnpjs).SiteCnpj(page);

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
    await csv.DisposeAsync();
    //new ConverterCsvService(pastaTemp).ExportarParaExcel(csvPath);
    await log.WriteLineAsync("------------------------------------------------------------");
    await log.WriteLineAsync(msgExecucao);
    await log.DisposeAsync();
}