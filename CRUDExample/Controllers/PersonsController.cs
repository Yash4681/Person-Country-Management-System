using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    public class PersonsController : Controller
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(IPersonsService personsService, ICountriesService countriesService, ILogger<PersonsController> logger)
        {
            _personsService = personsService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [Route("/")]
        [Route("persons/index")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortingOptions sortOption = SortingOptions.ASC)
        {
            _logger.LogInformation("Index method is called from PersonsController");
            _logger.LogDebug($"Parameters are: searchBy = {searchBy}, searchString = {searchString}, sortBy = {sortBy}, sortOption = {sortOption}");

            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name" },
                {nameof(PersonResponse.Email), "Email" },
                {nameof(PersonResponse.Address), "Address" },
                {nameof(PersonResponse.Gender), "Gender" },
                {nameof(PersonResponse.CountryID), "Country" },
                {nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                {nameof(PersonResponse.ReceiveNewsLetter), "Receive News Letter" }
            };

            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            List<PersonResponse> filteredPersons = await _personsService.GetFilteredPersons(searchBy, searchString);

            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOption = sortOption.ToString();

            List<PersonResponse> sortedPersons = await _personsService.GetSortedPersons(filteredPersons, sortBy, sortOption);

            return View(sortedPersons);
        }

        [Route("persons/create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("Create(Get) method is called from PersonsController");

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString()});

            return View();
        }

        [Route("persons/create")]
        [HttpPost]
        public async Task<IActionResult> Create(PersonAddRequest? personAddRequest)
        {
            _logger.LogInformation("Create(Post) method is called from PersonsController");
            _logger.LogDebug($"personAddRequest: {personAddRequest}");

            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = await _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(temp =>
                    new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(e => e.ErrorMessage).ToList();

                return View(personAddRequest);
            }
            await _personsService.AddPerson(personAddRequest);
            return RedirectToAction("Index", "Persons");
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Edit(Guid personID)
        {
            _logger.LogInformation("Edit(Get) method is called from PersonsController");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);
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
        public async Task<IActionResult> Edit(PersonUpdateRequest? personUpdateRequest)
        {
            _logger.LogInformation("Edit(Post) method is called from PersonsController");
            _logger.LogDebug($"personUpdateRequest: {personUpdateRequest}");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest?.PersonID);

            if (personResponse == null)
            {
                return RedirectToAction("Index");
            }

            List<CountryResponse> countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            if (ModelState.IsValid)
            {
                PersonResponse updatedPerson = await _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }
            else
            {                
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(e => e.ErrorMessage).ToList();

                return View(personResponse?.ToPersonUpdateRequest());
            }
        }

        [HttpGet]
        [Route("[action]/{personID}")]
        public async Task<IActionResult> Delete(Guid? personID)
        {
            _logger.LogInformation("Delete(Get) method is called from PersonsController");
            _logger.LogDebug($"personUpdateRequest: {personID}");

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);
            if(personResponse == null)
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

            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);
            if( personResponse == null)
            {
                return RedirectToAction("Index");
            }

            await _personsService.DeletePerson(personUpdateRequest.PersonID);

            return RedirectToAction("Index");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        {
            _logger.LogInformation("PersonsPDF method is called from PersonsController");

            List<PersonResponse> personResponses = await _personsService.GetAllPersons();

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

            MemoryStream memoryStream = await _personsService.GetPersonsCsv();
            return File(memoryStream, "application/octet-stream", "Persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            _logger.LogInformation("PersonsExcel method is called from PersonsController");

            MemoryStream memoryStream = await _personsService.GetPersonsExcel();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Persons.xlsx");
        }
    }
}
