using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System;
using System.Globalization;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsService(IPersonsRepository personsRepository, ILogger<PersonsService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            _logger.LogInformation("AddPerson method is called from PersonsService");
            _logger.LogDebug($"personAddRequest: {personAddRequest}");

            if (personAddRequest == null) throw new ArgumentNullException(nameof(personAddRequest));

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();
            person.PersonID = Guid.NewGuid();

            await _personsRepository.AddPerson(person);

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method is called from PersonsService");

            return (await _personsRepository.GetAllPersons()).Select(temp => temp.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            _logger.LogInformation("GetPersonByPersonID method is called from PersonsService");
            _logger.LogDebug($"personID: {personID}");

            if (personID == null) return null;

            Person? person = await _personsRepository.GetPersonByPersonId(personID.Value);

            if (person == null) return null;

            PersonResponse personResponse = person.ToPersonResponse();
            return personResponse;
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
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

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string? sortBy, SortingOptions sortingOptions)
        {
            _logger.LogInformation("GetSortedPersons method is called from PersonsService");
            _logger.LogDebug($"allPersons: {allPersons}, sortBy: {sortBy}, sortingOptions: {sortingOptions}");

            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }

            List<PersonResponse> sortedPersons = new List<PersonResponse>();

            switch (sortBy)
            {
                case nameof(PersonResponse.PersonName):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.Email):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.DateOfBirth):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.DateOfBirth).ToList() :
                        allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList();
                    break;
                case nameof(PersonResponse.Gender):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.Address):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.ReceiveNewsLetter):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.ReceiveNewsLetter).ToList() :
                        allPersons.OrderByDescending(temp => temp.ReceiveNewsLetter).ToList();
                    break;
                case nameof(PersonResponse.Age):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Age).ToList() :
                        allPersons.OrderByDescending(temp => temp.Age).ToList();
                    break;
                case nameof(PersonResponse.Country):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                default:
                    sortedPersons = allPersons;
                    break;
            }

            return sortedPersons;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            _logger.LogInformation("UpdatePerson method is called from PersonsService");
            _logger.LogDebug($"personUpdateRequest: {personUpdateRequest}");

            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            ValidationHelper.ModelValidation(personUpdateRequest);

            Person? matchingPerson = await _personsRepository.GetPersonByPersonId(personUpdateRequest.PersonID);

            if(matchingPerson == null)
                throw new InvalidPersonIDException("Given personID is invalid");

            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetter = personUpdateRequest.ReceiveNewsLetter;

            matchingPerson = await _personsRepository.UpdatePerson(matchingPerson);
            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            _logger.LogInformation("DeletePerson method is called from PersonsService");
            _logger.LogDebug($"personID: {personID}");

            if (personID == null) throw new ArgumentNullException(nameof(personID));
            if (_personsRepository.GetPersonByPersonId(personID.Value) == null)
            {
                return false;
            }
            return await _personsRepository.DeletePerson(personID.Value);
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

        public async Task<MemoryStream> GetPersonsCsv()
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

        public async Task<MemoryStream> GetPersonsExcel()
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
