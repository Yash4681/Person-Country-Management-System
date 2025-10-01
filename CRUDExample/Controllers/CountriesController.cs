using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {
        private readonly ICountriesService _countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult UploadFromExcel()
        {
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if(excelFile == null || excelFile.Length == 0 || !Path.GetExtension(excelFile.FileName).Equals(".xlsx"))
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
