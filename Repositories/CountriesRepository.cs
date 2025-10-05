using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace Repositories
{
    public class CountriesRepository : ICountriesRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CountriesRepository> _logger;

        public CountriesRepository(ApplicationDbContext db, ILogger<CountriesRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Country> AddCountry(Country country)
        {
            _logger.LogInformation("AddCountry method is called from CountriesRepository");
            _logger.LogDebug($"country: {country}");

            _db.Add(country);
            await _db.SaveChangesAsync();
            return country;
        }

        public async Task<List<Country>> GetAllCountries()
        {
            _logger.LogInformation("GetAllCountries method is called from CountriesRepository");

            return await _db.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryByCountryID(Guid countryID)
        {
            _logger.LogInformation("GetCountryByCountryID method is called from CountriesRepository");
            _logger.LogDebug($"countryID: {countryID}");

            return await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryID == countryID);
        }

        public async Task<Country?> GetCountryByCountryName(string countryName)
        {
            _logger.LogInformation("GetCountryByCountryName method is called from CountriesRepository");
            _logger.LogDebug($"countryName: {countryName}");

            return await _db.Countries.FirstOrDefaultAsync(temp => temp.CountryName == countryName);
        }
    }
}
