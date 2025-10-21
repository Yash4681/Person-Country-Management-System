using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
    public class PersonResponse
    {
        public Guid PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Gender { get; set; }
        public double? Age { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetter { get; set; }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest
            {
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetter = ReceiveNewsLetter,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender, true),
                PersonID = PersonID
            };
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if(obj.GetType() != typeof(PersonResponse)) return false;
            PersonResponse person = (PersonResponse)obj;
            return PersonID == person.PersonID && PersonName == person.PersonName && 
                Email == person.Email && DateOfBirth == person.DateOfBirth && 
                CountryID == person.CountryID && Gender == person.Gender && Age == person.Age && 
                Address == person.Address && Country == person.Country && 
                ReceiveNewsLetter == person.ReceiveNewsLetter;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person ID: {PersonID}, Person Name: {PersonName}, Email: {Email}, Date Of Birth: {DateOfBirth?.ToString("dd MMM yyyy")}, Country ID: {CountryID}, Country: {Country}, Gender: {Gender}, Age: {Age}, Address: {Address}, Recieve News Letter: {ReceiveNewsLetter}";
        }
    }

    public static class PersonExtentions
    {
        public static PersonResponse ToPersonResponse(this Person person)
        {
            return new PersonResponse()
            {
                PersonID = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                CountryID = person.CountryID,
                Gender = person.Gender,
                Address = person.Address,
                ReceiveNewsLetter = person.ReceiveNewsLetter,
                Age = (person.DateOfBirth != null) ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null,
                Country = person.Country?.CountryName
            };
        }
    }
}
