using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDUnitTest
{
    public class PersonsControllerTest
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ICountriesService _countriesService;
        private readonly Fixture _fixture;
        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        public PersonsControllerTest()
        {
            _fixture = new Fixture();
            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsSorterServiceMock = new Mock<IPersonsSorterService>();
            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _countriesServiceMock = new Mock<ICountriesService>();
            _personsGetterService = _personsGetterServiceMock.Object;
            _personsAdderService = _personsAdderServiceMock.Object;
            _personsDeleterService = _personsDeleterServiceMock.Object;
            _personsSorterService = _personsSorterServiceMock.Object;
            _personsUpdaterService = _personsUpdaterServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
            _loggerMock = new Mock<ILogger<PersonsController>>();
        }

        [Fact]
        public async Task Index_ShouldReturnIndexViewWitPersonsList()
        {
            //Arrange
            List<PersonResponse> personResponses = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personResponses);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortingOptions>())).ReturnsAsync(personResponses);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortingOptions>());

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);

            action.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();

            action.ViewData.Model.Should().Be(personResponses);
        }

        [Fact]
        public async Task Create_IfNoModelError_ToReturnRedirectToActionView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);

            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_IfGetRequest_ToReturnCreateView()
        {
            //Arrange
            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            //Act
            personsController.ModelState.AddModelError("PersonName", "Person name can't be blank");

            IActionResult result = await personsController.Create();

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_IfGetRequestWithValidPersonID_ToReturnEditView()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Edit(personID);

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);
            action.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            action.ViewData.Model.Should().BeEquivalentTo(personUpdateRequest);
        }

        [Fact]
        public async Task Edit_IfGetRequestWithInvalidPersonID_ToReturnRedirectToActionResult()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

            //Act
            IActionResult result = await personsController.Edit(personID);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_IfPostRequestWithInvalidPersonID_ToReturnRedirectToActionResult()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

            //Act
            IActionResult result = await personsController.Edit(personUpdateRequest);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_IfPostRequestWithValidPersonUpdateRequest_ToReturnRedirectToActionResult()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = _fixture.Create<PersonUpdateRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            _personsUpdaterServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Edit(personUpdateRequest);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_IfGetRequestWithValidPersonID_ToReturnDeleteView()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Delete(personID);

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);
            action.ViewData.Model.Should().BeAssignableTo<PersonResponse>();
            action.ViewData.Model.Should().BeEquivalentTo(personResponse);
        }

        [Fact]
        public async Task Delete_IfGetRequestWithInvalidPersonID_ToReturnRedirectToActionResult()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

            //Act
            IActionResult result = await personsController.Delete(personID);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_IfPostRequestWithValidPersonID_ToReturnRedirectToActionResult()
        {
            //Arrange
            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);
            _personsDeleterServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            IActionResult result = await personsController.Delete(personUpdateRequest);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_IfPostRequestWithInvalidPersonID_ToReturnRedirectToActionResult()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            PersonsController personsController = new PersonsController(_personsGetterService, _personsAdderService, _personsDeleterService, _personsSorterService, _personsUpdaterService, _countriesService, _loggerMock.Object);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

            //Act
            IActionResult result = await personsController.Delete(personID);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }
    }
}
