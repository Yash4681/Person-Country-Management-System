using Microsoft.Extensions.Logging;
using RepositoryContracts;
using Serilog;
using ServiceContracts;

namespace Services
{
    public class PersonsDeleterService : IPersonsDeleterService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsDeleterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsDeleterService(IPersonsRepository personsRepository, ILogger<PersonsDeleterService> logger, IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            _logger.LogInformation("DeletePerson method is called from PersonsService");
            _logger.LogDebug($"personID: {personID}");

            if (personID == null) throw new ArgumentNullException(nameof(personID));
            if (_personsRepository.GetPersonByPersonId(personID.Value) == null)
            {
                return false;
            }
            return await _personsRepository.DeletePerson(personID.Value);
        }
    }
}
