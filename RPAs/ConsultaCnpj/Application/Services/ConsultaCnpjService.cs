using PuppeteerSharp;
using System.Data;
using System.Text.RegularExpressions;

namespace Application.Services;

public class ConsultaCnpjService (StreamWriter logFile, StreamWriter csvFile, string baseUrl, List<string> listaCnpjs)
{
    private StreamWriter LogFile { get; set; } = logFile;
    private StreamWriter CsvFile { get; set; } = csvFile;
    private string Url { get; set; } = baseUrl;
    private List<string> ListaCnpjs { get; set; } = listaCnpjs;

    public async Task<DataTable> SiteCnpj(IPage page)
    {
        DataTable tabelaCnpjs = new();
        tabelaCnpjs.Columns.Add("Cnpj", typeof(string));
        tabelaCnpjs.Columns.Add("Status", typeof(string));

        await CsvFile.WriteLineAsync("Cnpj;Status");

        foreach (string cnpj in ListaCnpjs)
        {
            if (!Regex.IsMatch(cnpj, @"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$"))
            {
                tabelaCnpjs.Rows.Add(cnpj, "Cnpj não está no formato correto.");
                continue;
            }
            string digitosCnpj = Regex.Replace(cnpj, @"\D", "");

            await ConsultaCnpj(page, digitosCnpj);
        }

        return tabelaCnpjs;
    }
    private static async Task<bool> ConsultaCnpj(IPage page, string cnpj)
    {
        if (!await OpenCnpjSite(page))
        {
            throw new SiteNavigationException("Erro ao abrir o site da Fipe.");
        }
        return true;
    }
    private async Task<bool> OpenCnpjSite(IPage page)
    {
        int attempts = 0;
        while (attempts < 5)
        {
            try
            {
                await page.GoToAsync(Url, 20000, WaitUntilNavigation.DOMContentLoaded);
                if (await IsSiteLoaded(page))
                    return true;
            }
            catch
            {
                attempts++;
                await Task.Delay(5000);
            }
        }
        return false;
    }
    private static async Task<bool> IsSiteLoaded(IPage page)
    {
        var elements = await page.XPathAsync("//*[@id=\"app\"]/div[1]/div/div/div/div/div");
        return elements.Length > 0;
    }
}
