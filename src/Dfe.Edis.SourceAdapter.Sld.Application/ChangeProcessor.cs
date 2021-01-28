using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Application
{
    public interface IChangeProcessor
    {
        Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken);

        Task ProcessProviderAsync(string academicYear, int ukprn, CancellationToken cancellationToken);
    }

    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IStateStore _stateStore;
        private readonly ISldClient _sldClient;
        private readonly IProviderQueue _providerQueue;
        private readonly ILogger<ChangeProcessor> _logger;

        public ChangeProcessor(
            IStateStore stateStore,
            ISldClient sldClient,
            IProviderQueue providerQueue,
            ILogger<ChangeProcessor> logger)
        {
            _stateStore = stateStore;
            _sldClient = sldClient;
            _providerQueue = providerQueue;
            _logger = logger;
        }

        public async Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting academic years from SLD");
            var academicYears = await _sldClient.ListAcademicYearsAsync(cancellationToken);
            if (!academicYears.Any())
            {
                _logger.LogInformation("No academic years returned from SLD. Exiting check");
                return;
            }

            var academicYear = academicYears.First();

            _logger.LogInformation("Getting last time providers were checked");
            var lastPoll = await _stateStore.GetLastPollTimeAsync(cancellationToken);

            var providers = new List<int>();
            var hasMorePages = true;
            var pageNumber = 1;
            while (hasMorePages)
            {
                _logger.LogInformation("Getting providers that have submitted since {LastPoll} in academic year {AcademicYear}",
                    lastPoll, academicYear);
                var pageOfProviders = await _sldClient.ListProvidersThatHaveSubmittedSinceAsync(academicYear, lastPoll, pageNumber, cancellationToken);

                providers.AddRange(pageOfProviders.Items);
                pageNumber++;
                hasMorePages = pageNumber <= pageOfProviders.TotalNumberOfPages;
            }

            lastPoll = DateTime.Now;
            _logger.LogInformation("Found {NumberOfProviders} providers changed since {LastPoll} in academic year {AcademicYear}",
                providers.Count, lastPoll, academicYear);

            foreach (var provider in providers)
            {
                await _providerQueue.EnqueueAsync(new ProviderQueueItem
                {
                    AcademicYear = academicYear,
                    Ukprn = provider,
                }, cancellationToken);
                _logger.LogInformation("Queued provider {UKPRN} from academic year {AcademicYear} for processing",
                    provider, academicYear);
            }

            await _stateStore.SetLastPollTimeAsync(lastPoll.Value, cancellationToken);
        }

        public async Task ProcessProviderAsync(string academicYear, int ukprn, CancellationToken cancellationToken)
        {
            var firstPageOfLearners = await _sldClient.ListLearnersForProviderAsync(academicYear, ukprn, 1, cancellationToken);

            // TODO: Check if number of learners in tolerance

            // TODO: Queue item for each page 
        }
    }
}