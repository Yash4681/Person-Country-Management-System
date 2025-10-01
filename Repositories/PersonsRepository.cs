using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using System.Linq.Expressions;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;

        public PersonsRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _db.Add(person);
            await _db.SaveChangesAsync();
            return person;
        }

        public async Task<bool> DeletePerson(Guid personID)
        {
            _db.Persons.RemoveRange(_db.Persons.Where(temp => temp.PersonID == personID));
            int rowsUpdated = await _db.SaveChangesAsync();
            return rowsUpdated > 0;
        }

        public async Task<List<Person>> GetAllPersons()
        {
            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            return await _db.Persons.Include("Country").Where(predicate).ToListAsync();
        }

        public async Task<Person?> GetPersonByPersonId(Guid personID)
        {
            return await _db.Persons.Include("Country").FirstOrDefaultAsync(temp => temp.PersonID == personID);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
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
