using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    public class PersonsAlwaysRunResultFilter : IAlwaysRunResultFilter
    {
        private readonly ILogger<PersonsAlwaysRunResultFilter> _logger;

        public PersonsAlwaysRunResultFilter(ILogger<PersonsAlwaysRunResultFilter> logger)
        {
            _logger = logger;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsAlwaysRunResultFilter), nameof(OnResultExecuted));
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Filters.OfType<SkipFilter>().Any())
            {
                return;
            }
            _logger.LogInformation("{FilterName}.{MethodName} method", nameof(PersonsAlwaysRunResultFilter), nameof(OnResultExecuting));

        }
    }
}
