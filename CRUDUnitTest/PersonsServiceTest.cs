using AutoFixture;
using Entities;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace CRUDUnitTest
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();

            #region DbMock
            //List<Country> countries = new List<Country>();
            //List<Person> Persons = new List<Person>();

            //DbContextMock<ApplicationDbContext> dbContext = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            //dbContext.CreateDbSetMock(temp => temp.Countries, countries);
            //dbContext.CreateDbSetMock(temp => temp.Persons, Persons);

            //ApplicationDbContext applicationDbContext = dbContext.Object;
            #endregion

            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            _personsService = new PersonsService(_personsRepository);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson

        //When the PersonAddRequest is null, It should throw ArgumentNullException.
        [Fact]
        public async Task AddPerson_NullPersonAddRequest_ToBeArgumentNullException()
        {
            //Arrange
            PersonAddRequest personAddRequest = null;

            //Act
            Func<Task> action = async () =>
            {
                PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When the PersonName is null, It should throw ArgumentException.
        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.PersonName, null as string).Create();

            //Act
            Func<Task> action = async () =>
            {
                PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the proper PersonAddRequest is supplied, It should return the valid PersonResponse object with the valid PersonID.
        [Fact]
        public async Task AddPerson_FullPersonAddRequest_ToBeSuccessfull()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>().With(temp => temp.Email, "Test@email.com").Create();
            Person? person = personAddRequest.ToPerson();

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);
           
            PersonResponse expectedPersonsResponse = person.ToPersonResponse();

            //Act
            PersonResponse actualPersonResponse = await _personsService.AddPerson(personAddRequest);
            expectedPersonsResponse.PersonID = actualPersonResponse.PersonID;

            //Assert
            actualPersonResponse.PersonID.Should().NotBeEmpty();
            actualPersonResponse.Should().Be(expectedPersonsResponse);
        }

        #endregion

        #region GetPersonByPersonID

        //When the PersonID is null, it should return null.
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? request = null;

            //Act
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(request);

            //Assert
            personResponse.Should().BeNull();
        }

        //When the PersonID is valid, it should return the matching PersonResponse object from List of Person.
        [Fact]
        public async Task GetPersonByPersonID_ValidPersonID_ToBeSuccessfull()
        {
            //Arrange
            Person person = _fixture.Build<Person>().With(temp => temp.Email, "Test@email.com").With(temp => temp.Country, null as Country).Create();

            PersonResponse expectedPersonResponse = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse? actualPersonResponse = await _personsService.GetPersonByPersonID(expectedPersonResponse.PersonID);

            //Assert
            actualPersonResponse.Should().NotBeNull();
            actualPersonResponse.Should().Be(expectedPersonResponse);
        }
        #endregion

        #region GetAllPersons

        //When no persons are added, it should return empty list
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList()
        {
            //Arrange
            List<Person> responses = new List<Person>();
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(responses);

            //Act
            List<PersonResponse> personResponses = await _personsService.GetAllPersons();

            //Assert
            personResponses.Should().BeEmpty();
        }

        //When few persons are added, it should return list of PersonResponse containing the added persons.
        [Fact]
        public async Task GetAllPersons_AddFewPersons_ToBeSuccessfull()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Asish")
                    .With(temp => temp.Email, "ashish@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Gita")
                    .With(temp => temp.Email, "gita@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Sid")
                    .With(temp => temp.Email, "sid@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            List<PersonResponse> expectedPersonResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //Act
            List<PersonResponse> actualPersonResponses = await _personsService.GetAllPersons();

            //Print all persons from Get list(Actual)
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse response in actualPersonResponses)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            actualPersonResponses.Should().BeEquivalentTo(expectedPersonResponse);
        }
        #endregion

        #region GetFilteredPersons

        //When the search string is empty, it should reaturn all the entries in list.
        [Fact]
        public async Task GetFilteredPersons_EmptySearchString_ToBeSuccessfull()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Asish")
                    .With(temp => temp.Email, "ashish@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Gita")
                    .With(temp => temp.Email, "gita@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Sid")
                    .With(temp => temp.Email, "sid@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> expectedPersonResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //Act
            List<PersonResponse> actualPersonResponse = await _personsService.GetFilteredPersons(nameof(Person.PersonName),"");

            //Print all persons from Get list(Actual)
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse response in actualPersonResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            actualPersonResponse.Should().BeEquivalentTo(expectedPersonResponse);
        }

        //When the search string is entered, it should reaturn all the entries in list which matches with the entered search string.
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Asish")
                    .With(temp => temp.Email, "ashish@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Sid")
                    .With(temp => temp.Email, "sid@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> expectedPersonResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //Act
            List<PersonResponse> actualPersonResponse = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "si");

            //Print all persons from Get list(Actual)
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse response in actualPersonResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            actualPersonResponse.Should().OnlyContain(temp => temp.PersonName.Contains("si", StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region GetSortedPersons
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Asish")
                    .With(temp => temp.Email, "ashish@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Gita")
                    .With(temp => temp.Email, "gita@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),
                _fixture.Build<Person>()
                    .With(temp => temp.PersonName, "Sid")
                    .With(temp => temp.Email, "sid@email.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> personResponsesFromGet = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortingOptions.DESC);

            //Print all persons from Get list(Actual)
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse response in personResponsesFromGet)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            personResponsesFromGet.Should().BeInDescendingOrder(temp => temp.PersonName);
        }
        #endregion

        #region UpdatePerson

        //When the PersonUpdateRequest is null, It should throw ArgumentNullException.
        [Fact]
        public async Task UpdatePerson_NullPersonUpdateRequest_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Act
            Func<Task> action = async () =>
            {
                PersonResponse personResponse = await _personsService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When the PersonUpdateRequest is invalid, It should throw ArgumentException.
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = new PersonUpdateRequest()
            {
                PersonID = Guid.NewGuid()
            };

            //Assert
            Func<Task> action = async () =>
            {
                //Act
                PersonResponse personResponse = await _personsService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the PersonName is null, It should throw ArgumentException.
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            Person person = _fixture.Build<Person>().With(temp => temp.Country, null as Country).With(temp => temp.Email, "Test@email.com").With(temp => temp.PersonName, null as string).With(temp => temp.Gender, "Male").Create();

            PersonResponse personResponse = person.ToPersonResponse();
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            Func<Task> action = async () =>
            {
                PersonResponse personResponse = await _personsService.UpdatePerson(personUpdateRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When the proper PersonUpdateRequest is supplied, It should return the valid PersonResponse object with the updated PersonResponse.
        [Fact]
        public async Task UpdatePerson_ProperPersonUpdateRequest_ToBeSuccessfull()
        {
            //Arrange
            Person person = _fixture.Build<Person>().With(temp => temp.Country, null as Country).With(temp => temp.Email, "Test@email.com").With(temp => temp.Gender, "Male").Create();

            PersonResponse expectedPersonResponse = person.ToPersonResponse();
            PersonUpdateRequest personUpdateRequest = expectedPersonResponse.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse actualPersonResponse = await _personsService.UpdatePerson(personUpdateRequest);
            _testOutputHelper.WriteLine("Expected: " + expectedPersonResponse);
            _testOutputHelper.WriteLine("Actual: " + actualPersonResponse);

            //Assert
            actualPersonResponse.Should().Be(expectedPersonResponse);
        }
        #endregion

        #region DeletePerson

        //When the PersonID is valid, It should return true.
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeTrue()
        {
            //Arrange
            Guid personID = Guid.NewGuid();
            _personsRepositoryMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            bool isDeleted = await _personsService.DeletePerson(personID);

            //Assert
            isDeleted.Should().BeTrue();
        }

        //When the PersonID is invalid, It should return false.
        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeFalse()
        {
            Guid personID = Guid.NewGuid();
            _personsRepositoryMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(false);
            //Act
            bool isDeleted = await _personsService.DeletePerson(personID);

            //Assert
            isDeleted.Should().BeFalse();
        }
        #endregion
    }
}
