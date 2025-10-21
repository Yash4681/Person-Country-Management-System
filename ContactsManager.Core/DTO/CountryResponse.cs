using Entities;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// CountryResponse DTO for Response object.
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null) return false;
            if(obj.GetType() != typeof(CountryResponse)) return false;
            CountryResponse? other = obj as CountryResponse;
            return CountryID == other?.CountryID && CountryName == other.CountryName;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
    public static class CountryExtention
    {
        /// <summary>
        /// ToCountryResponse extention for the Country Entity.
        /// </summary>
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse { CountryID = country.CountryID, CountryName = country.CountryName };
        }
    }
}
