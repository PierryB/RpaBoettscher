using PuppeteerSharp;

namespace Application.Services;

public class HistoricoFipeService (StreamWriter logFile, StreamWriter csvFile, string mesConferencia)
{
    public StreamWriter LogFile { get; set; } = logFile;
    public StreamWriter CsvFile { get; set; } = csvFile;
    public string MesConferencia { get; set; } = mesConferencia.Trim();

    public async Task SiteFipe(IPage page)
    {
        if (String.IsNullOrEmpty(MesConferencia))
        {
            throw new Exception("O parâmetro 'mesConferencia' está vazio.");
        }

        string mesConferencia = MesConferencia;
        StreamWriter log = LogFile;
        StreamWriter csv = CsvFile;
        csv.WriteLine("Marca;Modelo;Ano;Valor");
        int countAbreNav = 0;
        bool isAbriuSite = false;
        while (countAbreNav < 5)
        {
            await page.GoToAsync("https://veiculos.fipe.org.br/", 20000,
            [
                WaitUntilNavigation.Load,
                WaitUntilNavigation.DOMContentLoaded
            ]);

            try
            {
                Thread.Sleep(5000);
                var isAbriuNavegador = await page.XPathAsync("//*[@id=\"banner\"]/div/h1");
                if (isAbriuNavegador.Length > 0)
                {
                    isAbriuSite = true;
                    break;
                }
            }
            catch
            {
                Thread.Sleep(5000);
                countAbreNav++;
                continue;
            }
        }
        if (!isAbriuSite)
        {
            throw new Exception("Erro ao abrir o site da Fipe.");
        }
        log.WriteLine("Abriu o site da Fipe com sucesso.");
        await page.DeleteCookieAsync();
        await page.SetCacheEnabledAsync(false);

        await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight);");
        Thread.Sleep(500);
        await page.EvaluateExpressionAsync("document.querySelector('#front > div.content > div.tab.vertical.tab-veiculos > ul > li:nth-child(1) > a > div.title').click();");
        Thread.Sleep(500);

        try
        {
            await page.EvaluateFunctionAsync(@"(mesConferencia) => {
                const selectElement = document.querySelector('#selectTabelaReferenciacarro');
    
                if (selectElement) {
                    for (let option of selectElement.options) {
                        if (option.text.includes(mesConferencia)) {
                            option.selected = true;
                            selectElement.dispatchEvent(new Event('change'));
                            break;
                        }
                    }
                }
            }", mesConferencia);
        }
        catch
        {
            throw new Exception($"Não foi possível selecionar o mês de {mesConferencia} para conferência.");
        }

        var marcas = await page.EvaluateFunctionAsync<string[]>(@"
            () => {
                const selectElement = document.querySelector('#selectMarcacarro');
                return Array.from(selectElement.options).map(option => option.text);
            }
        ");

        if (marcas.Length == 0)
        {
            throw new Exception("Não foi possível coletar as marcas dos veículos.");
        }

        bool isCarregouTab = false;
        int countMarcas = 0;
        foreach (var marca in marcas)
        {
            countMarcas++;
            if (String.IsNullOrEmpty(marca))
                continue;
            
            try
            {
                await page.EvaluateFunctionAsync(@"(marca) => {
                const selectElement = document.querySelector('#selectMarcacarro');
    
                if (selectElement) {
                    for (let option of selectElement.options) {
                        if (option.text.includes(marca)) {
                            option.selected = true;
                            selectElement.dispatchEvent(new Event('change'));
                            break;
                        }
                    }
                }
            }", marca);
            }
            catch
            {
                log.WriteLine($"Não foi possível selecionar a marca {marca}.");
                continue;
            }
            Thread.Sleep(500);

            var modelos = await page.EvaluateFunctionAsync<string[]>(@"
                () => {
                    const selectElement = document.querySelector('#selectAnoModelocarro');
                    return Array.from(selectElement.options).map(option => option.text);
                }
            ");
            if (modelos.Length == 0)
            {
                log.WriteLine($"Não encontrou nenhum modelo para a marca {marca}.");
                continue;
            }

            foreach (var modelo in modelos)
            {
                if (String.IsNullOrEmpty(modelo))
                    continue;

                try
                {
                    await page.EvaluateFunctionAsync(@"(marca) => {
                        const selectElement = document.querySelector('#selectMarcacarro');
    
                        if (selectElement) {
                            for (let option of selectElement.options) {
                                if (option.text.includes(marca)) {
                                    option.selected = true;
                                    selectElement.dispatchEvent(new Event('change'));
                                    break;
                                }
                            }
                        }
                    }", marca);
                }
                catch
                {
                    log.WriteLine($"Não foi possível selecionar a marca {marca}.");
                    continue;
                }
                Thread.Sleep(500);

                try
                {
                    await page.EvaluateFunctionAsync(@"(modelo) => {
                        const selectElement = document.querySelector('#selectAnoModelocarro');
    
                        if (selectElement) {
                            for (let option of selectElement.options) {
                                if (option.text.includes(modelo)) {
                                    option.selected = true;
                                    selectElement.dispatchEvent(new Event('change'));
                                    break;
                                }
                            }
                        }
                    }", modelo);
                }
                catch
                {
                    log.WriteLine($"Não foi possível selecionar o modelo {modelo}.");
                    continue;
                }
                Thread.Sleep(500);

                var anos = await page.EvaluateFunctionAsync<string[]>(@"
                    () => {
                        const selectElement = document.querySelector('#selectAnocarro');
                        return Array.from(selectElement.options).map(option => option.text);
                    }
                ");
                if (anos.Length == 0)
                {
                    log.WriteLine($"Não encontrou nenhum ano para o veículo {marca} {modelo}.");
                    continue;
                }

                foreach (var ano in anos)
                {
                    isCarregouTab = false;
                    if (String.IsNullOrEmpty(ano))
                        continue;

                    try
                    {
                        await page.EvaluateFunctionAsync(@"(ano) => {
                        const selectElement = document.querySelector('#selectAnocarro');
    
                        if (selectElement) {
                            for (let option of selectElement.options) {
                                if (option.text.includes(ano)) {
                                    option.selected = true;
                                    selectElement.dispatchEvent(new Event('change'));
                                    break;
                                }
                            }
                        }
                    }", ano);
                    }
                    catch
                    {
                        log.WriteLine($"Não foi possível selecionar o ano {ano}.");
                        continue;
                    }
                    Thread.Sleep(500);

                    await page.EvaluateExpressionAsync("document.querySelector(\"#buttonPesquisarcarro\").click()");
                    try
                    {
                        await page.WaitForSelectorAsync("#resultadoConsultacarroFiltros > table", new WaitForSelectorOptions
                        {
                            Timeout = 15000
                        });
                        isCarregouTab = true;
                    }
                    catch
                    {
                        bool isAlertaVisivel = await page.EvaluateFunctionAsync<bool>(@"
                            () => {
                                const alerta = document.querySelector('#mm-0 > div.modal.alert');
                                return alerta && getComputedStyle(alerta).display !== 'none';
                            }
                        ");

                        if (isAlertaVisivel)
                        {
                            await page.ClickAsync("#mm-0 > div.modal.alert > div.btnClose");
                            Thread.Sleep(10000);
                            await page.EvaluateExpressionAsync("document.querySelector(\"#buttonPesquisarcarro\").click()");
                        }
                    }

                    if (!isCarregouTab)
                    {
                        try
                        {
                            await page.WaitForSelectorAsync("#resultadoConsultacarroFiltros > table", new WaitForSelectorOptions
                            {
                                Timeout = 15000
                            });
                        }
                        catch
                        {
                            throw new Exception($"Erro ao pesquisar o veículo {marca} {modelo} {ano}.");
                        }
                    }

                    var resultado = await page.EvaluateFunctionAsync<string[]>(@"
                        () => {
                            const resultado = [];
                            resultado.push(document.querySelector('#resultadoConsultacarroFiltros > table > tbody > tr:nth-child(3) > td:nth-child(2)').innerText);
                            resultado.push(document.querySelector('#resultadoConsultacarroFiltros > table > tbody > tr:nth-child(4) > td:nth-child(2)').innerText);
                            resultado.push(document.querySelector('#resultadoConsultacarroFiltros > table > tbody > tr:nth-child(5) > td:nth-child(2)').innerText);
                            resultado.push(document.querySelector('#resultadoConsultacarroFiltros > table > tbody > tr:nth-child(8) > td:nth-child(2)').innerText);
                            return resultado;
                        }
                    ");

                    csv.WriteLine($"{resultado[0].Trim()};{resultado[1].Trim()};{resultado[2].Trim()};{resultado[3].Replace("R$","").Trim()}");
                    Thread.Sleep(2000);
                }
                await page.ClickAsync("#buttonLimparPesquisarcarro > a");
                Thread.Sleep(3000);
            }
        }
    }
}
