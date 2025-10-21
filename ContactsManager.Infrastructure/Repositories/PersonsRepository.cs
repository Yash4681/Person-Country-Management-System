using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.Linq.Expressions;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PersonsRepository> _logger;

        public PersonsRepository(ApplicationDbContext db, ILogger<PersonsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _logger.LogInformation("AddPerson method is called from PersonsRepository");
            _logger.LogDebug($"person: {person}");

            _db.Add(person);
            await _db.SaveChangesAsync();
            return person;
        }

        public async Task<bool> DeletePerson(Guid personID)
        {
            _logger.LogInformation("DeletePerson method is called from PersonsRepository");
            _logger.LogDebug($"personID: {personID}");

            _db.Persons.RemoveRange(_db.Persons.Where(temp => temp.PersonID == personID));
            int rowsUpdated = await _db.SaveChangesAsync();
            return rowsUpdated > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons method is called from PersonsRepository");

            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            _logger.LogInformation("GetFilteredPersons method is called from PersonsRepository");
            _logger.LogDebug($"predicate: {predicate}");

            return await _db.Persons.Include("Country").Where(predicate).ToListAsync();
        }

        public async Task<Person?> GetPersonByPersonId(Guid personID)
        {
            _logger.LogInformation("GetPersonByPersonId method is called from PersonsRepository");
            _logger.LogDebug($"personID: {personID}");

            return await _db.Persons.Include("Country").FirstOrDefaultAsync(temp => temp.PersonID == personID);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            _logger.LogInformation("UpdatePerson method is called from PersonsRepository");
            _logger.LogDebug($"person: {person}");

            Person? matcingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == person.PersonID);

            if (matcingPerson == null)
                return person;

            matcingPerson.PersonName = person.PersonName;
            matcingPerson.Email = person.Email;
            matcingPerson.DateOfBirth = person.DateOfBirth;
            matcingPerson.Gender = person.Gender;
            matcingPerson.CountryID = person.CountryID;
            matcingPerson.Country = person.Country;
            matcingPerson.Address = person.Address;
            matcingPerson.ReceiveNewsLetter = person.ReceiveNewsLetter;

            await _db.SaveChangesAsync();
            return matcingPerson;
        }
    }
}
