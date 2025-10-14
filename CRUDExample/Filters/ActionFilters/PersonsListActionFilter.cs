using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger) 
        { 
           _logger = logger; 
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted method");

            IDictionary<string, object?>? parameters = (IDictionary<string, object?>?)context.HttpContext.Items["arguments"];

            PersonsController personsController = (PersonsController)context.Controller;

            if (parameters != null)
            {
                if (parameters.ContainsKey("searchBy"))
                {
                    personsController.ViewBag.CurrentSearchBy = Convert.ToString(parameters["searchBy"]);
                }

                if (parameters.ContainsKey("searchString"))
                {
                    personsController.ViewBag.CurrentSearchString = Convert.ToString(parameters["searchString"]);
                }

                if (parameters.ContainsKey("sortBy"))
                {
                    personsController.ViewBag.CurrentSortBy = Convert.ToString(parameters["sortBy"]);
                }

                if (parameters.ContainsKey("sortOption"))
                {
                    personsController.ViewBag.CurrentSortOption = Convert.ToString(parameters["sortOption"]);
                }
            }

            personsController.ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name" },
                {nameof(PersonResponse.Email), "Email" },
                {nameof(PersonResponse.Address), "Address" },
                {nameof(PersonResponse.Gender), "Gender" },
                {nameof(PersonResponse.CountryID), "Country" },
                {nameof(PersonResponse.DateOfBirth), "Date of Birth" },
                {nameof(PersonResponse.ReceiveNewsLetter), "Receive News Letter" }
            };
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["arguments"] = context.ActionArguments;

            _logger.LogInformation("PersonsListActionFilter.OnActionExecuting method");

            if(context.ActionArguments.ContainsKey("searchBy"))
            {
                string? searchBy = Convert.ToString(context.ActionArguments["searchBy"]);

                if(!string.IsNullOrEmpty(searchBy))
                {
                    var searchOptions = new List<string>()
                    {
                        nameof(PersonResponse.PersonName),
                        nameof(PersonResponse.Email),
                        nameof(PersonResponse.DateOfBirth),
                        nameof(PersonResponse.Gender),
                        nameof(PersonResponse.CountryID),
                        nameof(PersonResponse.ReceiveNewsLetter),
                        nameof(PersonResponse.Address),
                    };

                    if(searchOptions.Any(temp => temp == searchBy) == false)
                    {
                        _logger.LogInformation("searchBy actual value is {searchBy}", searchBy);
                        context.ActionArguments["searchBy"] = nameof(PersonResponse.PersonName);
                        _logger.LogInformation("searchBy updated value is {searchBy}", context.ActionArguments["searchBy"]);
                    }
                }
            }
        }
    }
}
