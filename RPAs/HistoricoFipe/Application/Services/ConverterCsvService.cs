using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Globalization;

namespace Application.Services;

public class ConverterCsvService (string diretorioTemp)
{
    public string DiretorioTemp { get; set; } = diretorioTemp;
    
    public void ExportarParaExcel(string csvPath)
    {
        string diretorioTemp = DiretorioTemp ?? string.Empty;
        string caminhoExcel = $@"{diretorioTemp}\Excel Tabela Fipe.xlsx";

        var linhas = new List<string[]>();

        using StreamReader reader = new(csvPath);
        string linha;
        while ((linha = reader.ReadLine()) != null)
        {
            string[] colunas = linha.Split(';');
            linhas.Add(colunas);
        }
        reader.Close();
        reader.Dispose();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage pacote = new();
        var planilha = pacote.Workbook.Worksheets.Add("Tabela Fipe");

        for (int i = 0; i < linhas.Count; i++)
        {
            for (int j = 0; j < linhas[i].Length; j++)
            {
                string valorCelula = linhas[i][j];

                if (j == 3 && i > 0)
                {
                    if (decimal.TryParse(valorCelula, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, new CultureInfo("pt-BR"), out decimal valorNumerico))
                    {
                        planilha.Cells[i + 1, j + 1].Value = valorNumerico;
                        planilha.Cells[i + 1, j + 1].Style.Numberformat.Format = "R$ #,##0.00";
                    }
                    else
                    {
                        planilha.Cells[i + 1, j + 1].Value = valorCelula;
                    }
                }
                else
                {
                    planilha.Cells[i + 1, j + 1].Value = valorCelula;
                }
            }
        }
        int ultimaLinha = planilha.Dimension.End.Row;

        planilha.Column(1).AutoFit();
        planilha.Column(2).AutoFit();
        planilha.Column(3).AutoFit();
        planilha.Column(4).AutoFit();

        planilha.Cells[$"D2:D{ultimaLinha}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

        FileInfo arquivoExcel = new(caminhoExcel);
        pacote.SaveAs(arquivoExcel);
        pacote.Dispose();
    }
}
