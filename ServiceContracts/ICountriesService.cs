using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// ICountriesService interface with AddCountry method.
    /// </summary>
    public interface ICountriesService
    {
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);
        Task<List<CountryResponse>> GetAllCountries();
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);
        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
    }
}
