using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using ServiceContracts.DTO;

namespace Services
{
    public class PersonsGetterServiceChild : PersonsGetterService
    {
        private readonly ILogger<PersonsGetterService> _logger; 
        public PersonsGetterServiceChild(IPersonsRepository personsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext) : base(personsRepository, logger, diagnosticContext) {
            _logger = logger;
        }

        public override async Task<MemoryStream> GetPersonsExcel()
        {
            _logger.LogInformation("GetPersonsExcel method is called from PersonsGetterServiceChild");

            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet Worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                Worksheet.Cells["A1"].Value = "Person Name";
                Worksheet.Cells["B1"].Value = "Age";
                Worksheet.Cells["C1"].Value = "Gender";

                using (ExcelRange headerCells = Worksheet.Cells["A1:C1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerCells.Style.Font.Bold = true;
                }

                List<PersonResponse> persons = await GetAllPersons();

                int row = 2;
                foreach (PersonResponse person in persons)
                {
                    Worksheet.Cells[row, 1].Value = person.PersonName;
                    Worksheet.Cells[row, 2].Value = person.Age;
                    Worksheet.Cells[row, 3].Value = person.Gender;

                    row++;
                }

                Worksheet.Cells[$"A1:C{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
