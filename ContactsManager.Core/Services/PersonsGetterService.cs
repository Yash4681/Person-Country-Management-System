using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts;
using ServiceContracts.DTO;
using System.Globalization;

namespace Services
{
    public class PersonsGetterService : IPersonsGetterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsGetterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsGetterService(IPersonsRepository personsRepository, ILogger<PersonsGetterService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public virtual async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method is called from PersonsService");

            return (await _personsRepository.GetAllPersons()).Select(temp => temp.ToPersonResponse()).ToList();
        }

        public virtual async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            _logger.LogInformation("GetPersonByPersonID method is called from PersonsService");
            _logger.LogDebug($"personID: {personID}");

            if (personID == null) return null;

            Person? person = await _personsRepository.GetPersonByPersonId(personID.Value);

            if (person == null) return null;

            PersonResponse personResponse = person.ToPersonResponse();
            return personResponse;
        }

        public virtual async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons method is called from PersonsService");
            _logger.LogDebug($"searchBy: {searchBy}, searchString: {searchString}");

            using (Operation.Time("Time taken for the GetFilteredPersons from PersonsRepository"))
            {
                List<Person> persons = searchBy switch
                {
                    nameof(Person.PersonName) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.PersonName.Contains(searchString)),

                    nameof(Person.Email) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.Email.Contains(searchString)),

                    nameof(Person.DateOfBirth) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.DateOfBirth.Value.ToString("dd MMM yyy").Contains(searchString)),

                    nameof(Person.Gender) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.Gender.Equals(searchString)),

                    nameof(Person.Address) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.Address.Contains(searchString)),

                    nameof(Person.ReceiveNewsLetter) =>
                        await _personsRepository.GetFilteredPersons(temp =>
                        temp.ReceiveNewsLetter.ToString().Contains(searchString)),

                    _ =>
                        await _personsRepository.GetAllPersons()
                };

                _diagnosticContext.Set("Persons", persons);
                return persons.Select(temp => temp.ToPersonResponse()).ToList();
            }
        }

        //public async Task<MemoryStream> GetPersonsCsv()
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    StreamWriter streamWriter = new StreamWriter(memoryStream);

        //    CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture, leaveOpen: true);

        //    csvWriter.WriteHeader<PersonResponse>();
        //    csvWriter.NextRecord();

        //    List<PersonResponse> persons = await _db.Persons.Include("Country").Select(temp => temp.ToPersonResponse()).ToListAsync();

        //    await csvWriter.WriteRecordsAsync(persons);

        //    memoryStream.Position = 0;

        //    return memoryStream;
        //}

        public virtual async Task<MemoryStream> GetPersonsCsv()
        {
            _logger.LogInformation("GetPersonsCsv method is called from PersonsService");

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            CsvWriter csvWriter = new CsvWriter(streamWriter, csvConfiguration, leaveOpen: true);

            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetter));
            csvWriter.NextRecord();

            List<PersonResponse> persons = await GetAllPersons();

            foreach (PersonResponse person in persons)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth.HasValue)
                    csvWriter.WriteField(person.DateOfBirth.Value.ToString("dd-MMM-yyyy"));
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetter);
                csvWriter.NextRecord();
                await csvWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public virtual async Task<MemoryStream> GetPersonsExcel()
        {
            _logger.LogInformation("GetPersonsExcel method is called from PersonsService");

            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet Worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                Worksheet.Cells["A1"].Value = "Person Name";
                Worksheet.Cells["B1"].Value = "Email";
                Worksheet.Cells["C1"].Value = "Date of Birth";
                Worksheet.Cells["D1"].Value = "Age";
                Worksheet.Cells["E1"].Value = "Gender";
                Worksheet.Cells["F1"].Value = "Country";
                Worksheet.Cells["G1"].Value = "Address";
                Worksheet.Cells["H1"].Value = "Receive News Letter";

                using (ExcelRange headerCells = Worksheet.Cells["A1:H1"])
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
                    Worksheet.Cells[row, 2].Value = person.Email;
                    if (person.DateOfBirth.HasValue)
                        Worksheet.Cells[row, 3].Value = person.DateOfBirth.Value.ToString("dd-MM-yyyy");
                    Worksheet.Cells[row, 4].Value = person.Age;
                    Worksheet.Cells[row, 5].Value = person.Gender;
                    Worksheet.Cells[row, 6].Value = person.Country;
                    Worksheet.Cells[row, 7].Value = person.Address;
                    Worksheet.Cells[row, 8].Value = person.ReceiveNewsLetter;
                    row++;
                }

                Worksheet.Cells[$"A1:H{row}"].AutoFitColumns();

                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
