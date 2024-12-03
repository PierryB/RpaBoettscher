using OfficeOpenXml;

namespace Application.Services;

public class LerPlanilhaCnpjsService(string arquivoExcelCnpjs)
{
    private string ArquivoExcelCnpjs { get; set; } = arquivoExcelCnpjs;

    public List<string> ObterListaDeCnpjs()
    {
        List<string> cnpjList = [];
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage(new FileInfo(ArquivoExcelCnpjs)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            int totalCols = worksheet.Dimension.End.Column;

            int cnpjColIndex = -1;
            for (int col = 1; col <= totalCols; col++)
            {
                if (worksheet.Cells[1, col].Text.Equals("CNPJ", StringComparison.OrdinalIgnoreCase))
                {
                    cnpjColIndex = col;
                    break;
                }
            }

            if (cnpjColIndex == -1)
            {
                throw new Exception("A coluna 'CNPJ' não foi encontrada na planilha.");
            }

            int totalRows = worksheet.Dimension.End.Row;
            for (int row = 2; row <= totalRows; row++)
            {
                var cnpj = worksheet.Cells[row, cnpjColIndex].Text;
                if (!string.IsNullOrEmpty(cnpj))
                {
                    cnpjList.Add(cnpj);
                }
            }
        }

        return cnpjList;
    }
}
