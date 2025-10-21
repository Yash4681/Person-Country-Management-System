using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace Services
{
    public class PersonsSorterService : IPersonsSorterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsSorterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsSorterService(IPersonsRepository personsRepository, ILogger<PersonsSorterService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string? sortBy, SortingOptions sortingOptions)
        {
            _logger.LogInformation("GetSortedPersons method is called from PersonsService");
            _logger.LogDebug($"allPersons: {allPersons}, sortBy: {sortBy}, sortingOptions: {sortingOptions}");

            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }

            List<PersonResponse> sortedPersons = new List<PersonResponse>();

            switch (sortBy)
            {
                case nameof(PersonResponse.PersonName):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.Email):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.DateOfBirth):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.DateOfBirth).ToList() :
                        allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList();
                    break;
                case nameof(PersonResponse.Gender):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.Address):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                case nameof(PersonResponse.ReceiveNewsLetter):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.ReceiveNewsLetter).ToList() :
                        allPersons.OrderByDescending(temp => temp.ReceiveNewsLetter).ToList();
                    break;
                case nameof(PersonResponse.Age):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Age).ToList() :
                        allPersons.OrderByDescending(temp => temp.Age).ToList();
                    break;
                case nameof(PersonResponse.Country):
                    sortedPersons = sortingOptions == SortingOptions.ASC ?
                        allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList() :
                        allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList();
                    break;
                default:
                    sortedPersons = allPersons;
                    break;
            }

            return sortedPersons;
        }
    }
}
