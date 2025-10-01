using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// CountryAddRequest DTO for request object.
    /// </summary>
    public class CountryAddRequest
    {
        public string? CountryName { get; set; }
        public Country ToCountry()
        {
            return new Country { CountryName = CountryName };
        }
    }
}
