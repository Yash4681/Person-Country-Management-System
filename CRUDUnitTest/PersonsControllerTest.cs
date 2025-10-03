using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDUnitTest
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly Fixture _fixture;
        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;

        public PersonsControllerTest()
        {
            _fixture = new Fixture();
            _personsServiceMock = new Mock<IPersonsService>();
            _countriesServiceMock = new Mock<ICountriesService>();
            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
        }

        [Fact]
        public async Task Index_ShouldReturnIndexViewWitPersonsList()
        {
            //Arrange
            List<PersonResponse> personResponses = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _personsServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(personResponses);

            _personsServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortingOptions>())).ReturnsAsync(personResponses);

            //Act
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<SortingOptions>());

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);

            action.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();

            action.ViewData.Model.Should().Be(personResponses);
        }

        [Fact]
        public async Task Create_IfModelError_ToReturnCreateView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(personResponse);

            //Act
            personsController.ModelState.AddModelError("PersonName", "Person name can't be blank");

            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);
            action.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            action.ViewData.Model.Should().Be(personAddRequest);
        }

        [Fact]
        public async Task Create_IfNoModelError_ToReturnRedirectToActionView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();

            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(personResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Edit(personUpdateRequest);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_IfPostRequestWithModelError_ToReturnEditView()
        {
            //Arrange
            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            List<CountryResponse> countryResponses = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>())).ReturnsAsync(personResponse);

            //Act
            personsController.ModelState.AddModelError("PersonName", "PersonNam can't be blank");
            IActionResult result = await personsController.Edit(personUpdateRequest);

            //Assert
            ViewResult action = Assert.IsType<ViewResult>(result);
            action.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            action.ViewData.Model.Should().BeEquivalentTo(personUpdateRequest);
        }

        [Fact]
        public async Task Delete_IfGetRequestWithValidPersonID_ToReturnDeleteView()
        {
            //Arrange
            Guid personID = _fixture.Create<Guid>();

            PersonResponse personResponse = _fixture.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(personResponse);
            _personsServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true);

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

            PersonsController personsController = new PersonsController(_personsService, _countriesService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as PersonResponse);

            //Act
            IActionResult result = await personsController.Delete(personID);

            //Assert
            RedirectToActionResult action = Assert.IsType<RedirectToActionResult>(result);
            action.ActionName.Should().Be("Index");
        }
    }
}
