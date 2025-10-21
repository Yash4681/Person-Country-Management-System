using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesService> _logger;

        public CountriesService(ICountriesRepository countriesRepository, ILogger<CountriesService> logger)
        {
            _countriesRepository = countriesRepository;
            _logger = logger;
        }
        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            _logger.LogInformation("AddCountry method is called from CountriesService");
            _logger.LogDebug($"countryAddRequest: {countryAddRequest}");

            //Validate: countryAddRequest should not be null
            if (countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }

            //Validate: CountryName should not be null
            if(countryAddRequest.CountryName == null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            //Validate: CountryName should not be duplicate
            if(await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Country name already exists.");
            }

            Country country = countryAddRequest.ToCountry();
            country.CountryID = Guid.NewGuid();

            await _countriesRepository.AddCountry(country);

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            _logger.LogInformation("GetAllCountries method is called from CountriesService");

            return (await _countriesRepository.GetAllCountries()).Select(temp => temp.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            _logger.LogInformation("GetCountryByCountryID method is called from CountriesService");
            _logger.LogDebug($"countryID: {countryID}");

            if (countryID == null)
            {
                return null;
            }

            Country? country = await _countriesRepository.GetCountryByCountryID(countryID.Value);
            return country?.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            _logger.LogInformation("UploadCountriesFromExcelFile method is called from CountriesService");
            _logger.LogDebug($"formFile: {formFile}");

            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            int insertedCountries = 0;
            using(ExcelPackage package = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Countries"];
                int rowCount = worksheet.Dimension.Rows;
                for(int row = 2; row <= rowCount; row++)
                {
                    var cellValue = Convert.ToString(worksheet.Cells[row, 1].Value);
                    if(cellValue != null)
                    {
                        string? countryName = cellValue;
                        if(await _countriesRepository.GetCountryByCountryName(countryName) == null)
                        {
                            Country country = new Country()
                            {
                                CountryName = countryName,
                            };                            

                            await _countriesRepository.AddCountry(country);
                            insertedCountries++;
                        }
                    }
                }
            }
            return insertedCountries;
        }
    }
}
