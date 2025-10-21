using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorisationFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "Key-From-Controller", "Value-From-Controller", 3 }, Order = 3)]
    [ResponseHeaderFilterFactory("Key-From-Controller", "Value-From-Controller", 3)]
    //[TypeFilter(typeof(HandleExceptionFilter))]
    [TypeFilter(typeof(PersonsAlwaysRunResultFilter))]
    public class PersonsController : Controller
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsGetterService personsGetterService, IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService, IPersonsSorterService personsSorterService, IPersonsUpdaterService personsUpdaterService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsGetterService = personsGetterService;
            _personsAdderService = personsAdderService;
            _personsDeleterService = personsDeleterService;
            _personsSorterService = personsSorterService;
            _personsUpdaterService = personsUpdaterService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("/")]
        [Route("persons/index")]
        [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Index-Key-FromAction", "Index-Value-FromAction", 1 }, Order = 1)]
        [ResponseHeaderFilterFactory("X-Index-Key-FromAction", "Index-Value-FromAction", 1)]
        [TypeFilter(typeof(PersonsListResultFilter))]
        [SkipFilter]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortingOptions sortOption = SortingOptions.ASC)
        {
            _logger.LogInformation("Index method is called from PersonsController");
            _logger.LogDebug($"Parameters are: searchBy = {searchBy}, searchString = {searchString}, sortBy = {sortBy}, sortOption = {sortOption}");

            List<PersonResponse> filteredPersons = await _personsGetterService.GetFilteredPersons(searchBy, searchString);

            List<PersonResponse> sortedPersons = await _personsSorterService.GetSortedPersons(filteredPersons, sortBy, sortOption);

            return View(sortedPersons);
        }

        [Route("persons/create")]
        [HttpGet]
        //[TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Create-Key-FromAction", "Create-Value-FromAction", 1 })]
        [ResponseHeaderFilterFactory("X-Create-Key-FromAction", "Create-Value-FromAction", 1)]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create(Get) method is called from PersonsController");

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View();
        }

        [Route("persons/create")]
        [HttpPost]
        [TypeFilter(typeof(PersonsCreateEditPostActionFilter))]
        [TypeFilter(typeof(FeatureDisabledResourceFilter), Arguments = new object[] {false})]
        public async Task<IActionResult> Create(PersonAddRequest? personRequest)
        {
            _logger.LogInformation("Create(Post) method is called from PersonsController");
            _logger.LogDebug($"personAddRequest: {personRequest}");

            await _personsAdderService.AddPerson(personRequest);
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        [TypeFilter(typeof(TokenResultFilter))]
        public async Task<IActionResult> Edit(Guid personID)
        {
            _logger.LogInformation("Edit(Get) method is called from PersonsController");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        [TypeFilter(typeof(PersonsCreateEditPostActionFilter))]
        [TypeFilter(typeof(TokenAuthorisationFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest? personRequest)
        {
            _logger.LogInformation("Edit(Post) method is called from PersonsController");
            _logger.LogDebug($"personUpdateRequest: {personRequest}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personRequest?.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            PersonResponse updatedPerson = await _personsUpdaterService.UpdatePerson(personRequest);
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID)
        {
            _logger.LogInformation("Delete(Get) method is called from PersonsController");
            _logger.LogDebug($"personUpdateRequest: {personID}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            return View(personResponse);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest personUpdateRequest)
        {
            _logger.LogInformation("Delete(Post) method is called from PersonsController");
            _logger.LogDebug($"personUpdateRequest: {personUpdateRequest}");

            PersonResponse? personResponse = await _personsGetterService.GetPersonByPersonID(personUpdateRequest.PersonID);
            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            await _personsDeleterService.DeletePerson(personUpdateRequest.PersonID);

            return RedirectToAction("Index");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        {
            _logger.LogInformation("PersonsPDF method is called from PersonsController");

            List<PersonResponse> personResponses = await _personsGetterService.GetAllPersons();

            return new ViewAsPdf("PersonsPDF", personResponses, ViewData)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Left = 20,
                    Bottom = 20,
                    Right = 20,
                    Top = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
            };
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsCSV()
        {
            _logger.LogInformation("PersonsCSV method is called from PersonsController");

            MemoryStream memoryStream = await _personsGetterService.GetPersonsCsv();
            return File(memoryStream, "application/octet-stream", "Persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            _logger.LogInformation("PersonsExcel method is called from PersonsController");

            MemoryStream memoryStream = await _personsGetterService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Persons.xlsx");
        }
    }
}
