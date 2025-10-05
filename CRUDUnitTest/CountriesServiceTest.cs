using AutoFixture;
using Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace CRUDUnitTest
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly ICountriesRepository _countriesRepository;
        private readonly IFixture _fixture;
        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            #region DbMock
            //List<Country> countries = new List<Country>();

            //DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            //dbContextMock.CreateDbSetMock(temp => temp.Countries, countries);

            //ApplicationDbContext dbContext = dbContextMock.Object;
            #endregion

            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;

            var loggerMock = new Mock<ILogger<CountriesService>>();

            _countriesService = new CountriesService(_countriesRepository, loggerMock.Object);
        }

        #region AddCountry
        //if CountryAddRequest is null, AddCountry should throw ArgumentNullExeption. 
        [Fact]
        public async Task AddCountry_NullCountryAddRequest_ToBeArgumentNullException()
        {
            //Arrange
            CountryAddRequest countryAddRequest = null;

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(countryAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //if CountryName is null, AddCountry should throw ArgumentException.
        [Fact]
        public async Task AddCountry_NullCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string)
                .Create();

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(countryAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //if CountryName is duplicate, AddCountry should throw ArgumentException.
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "USA")
                .Create();
            Country country = countryAddRequest.ToCountry();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(country);

            //Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(countryAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //if CountryAddRequest is correct, AddCountry should return CountryResponse with valid Guid for CountryID.
        [Fact]
        public async Task AddCountry_CorrectCountryAddRequest_ToBeSuccessfull()
        {
            //Arrange
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            Country country = countryAddRequest.ToCountry();
            CountryResponse expectedCountryResponse = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>())).ReturnsAsync(null as Country);
            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            // Act
            CountryResponse actualCountryResponse = await _countriesService.AddCountry(countryAddRequest);
            expectedCountryResponse.CountryID = actualCountryResponse.CountryID;

            //Assert
            actualCountryResponse.Should().Be(expectedCountryResponse);
        }
        #endregion

        #region GetAllCountries
        //GetAllCountries should return a empty list of CountryResponse object when no country are added in list.
        [Fact]
        public async Task GetAllCountries_ToBeEmptyList()
        {
            //Arrange 
            List<Country> persons = new List<Country>();
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(persons);

            //Act
            List<CountryResponse> countryResponses = await _countriesService.GetAllCountries();

            //Assert
            countryResponses.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCountries_CountryResponseList_ToBeSuccessfull()
        {
            //Arrange
            List<Country> countries = new List<Country>()
            {
                _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create(),
                _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create()
            };

            List<CountryResponse> expectedCountries = countries.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);

            //Act
            List<CountryResponse> actualCountries = await _countriesService.GetAllCountries();

            //Assert
            actualCountries.Should().BeEquivalentTo(expectedCountries);
        }
        #endregion

        #region GetCountryByCountyID

        //if the CountryID is null, CountryResponse should be null.
        [Fact]
        public async Task GetCountryByCountryID_NullCountryID_ToBeNull()
        {
            //Arrange
            Guid? countryID = null;

            //Act
            CountryResponse? countryResponse = await _countriesService.GetCountryByCountryID(countryID);

            //Assert
            countryResponse.Should().BeNull();
        }

        //GetCountryByCountryID should return the matching CountryResponse object with respect to the upplied countryID.
        [Fact]
        public async Task GetCountryByCountryID_ProperCountryID_ToBeSuccessfull()
        {
            //Arrange
            Country country = _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create();

            CountryResponse expectedCountryResponse = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>())).ReturnsAsync(country);

            //Act
            CountryResponse? actualCountryResponse = await _countriesService.GetCountryByCountryID(country.CountryID);

            //Assert
            actualCountryResponse.Should().Be(expectedCountryResponse);
        }
        #endregion
    }
}