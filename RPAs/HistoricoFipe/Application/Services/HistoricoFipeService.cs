using PuppeteerSharp;
using System.Globalization;

namespace Application.Services;

public class HistoricoFipeService (StreamWriter logFile, StreamWriter csvFile, string mesConferencia, string baseUrl)
{
    private StreamWriter LogFile { get; set; } = logFile;
    private StreamWriter CsvFile { get; set; } = csvFile;
    private string MesConferencia { get; set; } = mesConferencia.Trim();
    private string Url { get; set; } = baseUrl;

    public async Task SiteFipe(IPage page)
    {
        ValidateMesConferencia();
        await CsvFile.WriteLineAsync("Marca;Modelo;Ano;Valor");

        if (!await OpenFipeSite(page))
        {
            throw new Exception("Erro ao abrir o site da Fipe.");
        }

        await LogFile.WriteLineAsync("Abriu o site da Fipe com sucesso.");
        await SelectReferenceMonth(page);

        var marcas = await FetchOptions(page, "#selectMarcacarro");
        foreach (var marca in marcas)
        {
            if (string.IsNullOrEmpty(marca)) continue;
            await ProcessBrand(page, marca);
            break;
        }
    }

    private void ValidateMesConferencia()
    {
        if (string.IsNullOrEmpty(MesConferencia))
        {
            throw new Exception("O parâmetro 'mesConferencia' está vazio.");
        }
        if (!DateTime.TryParseExact(MesConferencia, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            throw new Exception("O parâmetro 'mesConferencia' não está em um formato válido. Use 'MM/yyyy'.");
        }

        MesConferencia = parsedDate.ToString("MMMM/yyyy", new CultureInfo("pt-BR"));
    }

    private async Task<bool> OpenFipeSite(IPage page)
    {
        int attempts = 0;
        while (attempts < 5)
        {
            try
            {
                await page.GoToAsync(Url, 20000, [WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded]);
                if (await IsSiteLoaded(page))
                {
                    await page.EvaluateExpressionAsync("document.querySelector('#front > div.content > div.tab.vertical.tab-veiculos > ul > li:nth-child(1) > a > div.title').click();");
                    Thread.Sleep(500);
                    return true;
                }
            }
            catch
            {
                attempts++;
                Thread.Sleep(5000);
            }
        }
        return false;
    }

    private static async Task<bool> IsSiteLoaded(IPage page)
    {
        var elements = await page.XPathAsync("//*[@id=\"banner\"]/div/h1");
        return elements.Length > 0;
    }

    private async Task SelectReferenceMonth(IPage page)
    {
        try
        {
            await page.EvaluateFunctionAsync(@"(mesConferencia) => {
                const selectElement = document.querySelector('#selectTabelaReferenciacarro');
                Array.from(selectElement.options).forEach(option => {
                    if (option.text.includes(mesConferencia)) {
                        option.selected = true;
                        selectElement.dispatchEvent(new Event('change'));
                    }
                });
            }", MesConferencia);
        }
        catch
        {
            throw new Exception($"Não foi possível selecionar o mês de {MesConferencia} para conferência.");
        }
    }

    private static async Task<string[]> FetchOptions(IPage page, string selector)
    {
        return await page.EvaluateFunctionAsync<string[]>($@"
            () => {{
                const selectElement = document.querySelector('{selector}');
                return Array.from(selectElement.options).map(option => option.text.trim());
            }}
        ");
    }

    private async Task ProcessBrand(IPage page, string marca)
    {
        try
        {
            await SelectOption(page, "#selectMarcacarro", marca);
            var modelos = await FetchOptions(page, "#selectAnoModelocarro");
            foreach (var modelo in modelos)
            {
                if (string.IsNullOrEmpty(modelo)) continue;
                await ProcessModel(page, marca, modelo);
            }
        }
        catch
        {
            await LogFile.WriteLineAsync($"Não foi possível processar a marca {marca}.");
        }
    }

    private async Task ProcessModel(IPage page, string marca, string modelo)
    {
        try
        {
            await SelectOption(page, "#selectAnoModelocarro", modelo);
            var anos = await FetchOptions(page, "#selectAnocarro");
            foreach (var ano in anos)
            {
                if (string.IsNullOrEmpty(ano)) continue;
                await ProcessYear(page, marca, modelo, ano);
            }

            await page.EvaluateExpressionAsync("document.querySelector('#buttonLimparPesquisarcarro > a').click()");
            Thread.Sleep(3000);

            await SelectOption(page, "#selectMarcacarro", marca);
        }
        catch
        {
            await LogFile.WriteLineAsync($"Não foi possível processar o modelo {modelo}.");
        }
    }

    private async Task ProcessYear(IPage page, string marca, string modelo, string ano)
    {
        try
        {
            await SelectOption(page, "#selectAnocarro", ano);
            await SearchVehicle(page);
            Thread.Sleep(2000);

            var valor = await FetchVehicleData(page);
            if (!string.IsNullOrEmpty(valor))
            {
                await CsvFile.WriteLineAsync($"{marca};{modelo};{ano};{valor.Replace("R$", "").Trim()}");
            }
            else
            {
                await LogFile.WriteLineAsync($"Nenhum valor encontrado para o veículo {marca} {modelo} {ano}.");
            }
        }
        catch
        {
            await LogFile.WriteLineAsync($"Erro ao processar o ano {ano} para o veículo {marca} {modelo}.");
        }
    }

    private static async Task SelectOption(IPage page, string selector, string value)
    {
        await page.EvaluateFunctionAsync(@$"(value) => {{
            const selectElement = document.querySelector('{selector}');
            Array.from(selectElement.options).forEach(option => {{
                if (option.text.includes(value)) {{
                   option.selected = true;
                   selectElement.dispatchEvent(new Event('change'));
                }}
            }});
        }}", value);
        Thread.Sleep(500);
    }

    private static async Task SearchVehicle(IPage page)
    {
        try
        {
            await page.EvaluateExpressionAsync("document.querySelector('#buttonPesquisarcarro').click()");

            await page.WaitForSelectorAsync("#resultadoConsultacarroFiltros > table, #mm-0 > div.modal.alert", new WaitForSelectorOptions { Timeout = 15000 });

            bool isErrorVisible = await page.EvaluateFunctionAsync<bool>(@"
                () => {
                    const alerta = document.querySelector('#mm-0 > div.modal.alert');
                    return alerta && getComputedStyle(alerta).display !== 'none';
                }
            ");

            if (isErrorVisible)
            {
                await page.EvaluateExpressionAsync("document.querySelector('#mm-0 > div.modal.alert > div.btnClose').click()");
                Thread.Sleep(10000);

                await page.EvaluateExpressionAsync("document.querySelector('#buttonPesquisarcarro').click()");
                await page.WaitForSelectorAsync("#resultadoConsultacarroFiltros > table", new WaitForSelectorOptions { Timeout = 15000 });
            }
        }
        catch
        {
            throw new Exception("Erro ao tentar pesquisar o veículo.");
        }
    }

    private static async Task<string> FetchVehicleData(IPage page)
    {
        return await page.EvaluateFunctionAsync<string>(@"
            () => {
                const cell = document.querySelector('#resultadoConsultacarroFiltros > table > tbody > tr:nth-child(8) > td:nth-child(2)');
                return cell ? cell.innerText.trim() : null;
            }
        ");
    }
}
