using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {
        private readonly ICountriesService _countriesService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountriesService countriesService, ILogger<CountriesController> logger)
        {
            _countriesService = countriesService;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult UploadFromExcel()
        {
            _logger.LogInformation("Get UploadFromExcel method is called from CountriesController");
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            _logger.LogInformation("Post UploadFromExcel method is called from CountriesController");
            _logger.LogDebug($"excelFile: {excelFile}");

            if (excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
            {
                ViewBag.ErrorMessage = "Please select valid .xlsx file";
                return View();
            }

            int insetedCountries = await _countriesService.UploadCountriesFromExcelFile(excelFile);

            ViewBag.Message = $"{insetedCountries} countries uploaded";

            return View();
        }
    }
}
