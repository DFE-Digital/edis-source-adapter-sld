using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Application
{
    public interface IChangeProcessor
    {
        Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken);
    }

    public class ChangeProcessor : IChangeProcessor
    {
        private readonly IStateStore _stateStore;
        private readonly ILogger<ChangeProcessor> _logger;

        public ChangeProcessor(
            IStateStore stateStore,
            ILogger<ChangeProcessor> logger)
        {
            _stateStore = stateStore;
            _logger = logger;
        }
        
        public async Task CheckForUpdatedProvidersAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting last time providers were checked");
            var lastPollState = await _stateStore.GetLastPollTimeAsync(cancellationToken);

            var lastPoll = lastPollState.HasValue ? lastPollState.Value : DateTime.Today.AddDays(-1);
            // TODO: Get list of provider from SLD
            
            // TODO: Check if each provider is update since last time we checked it (just in case we dropped mid-process last time)
            // TODO: Queue providers
            
            lastPoll = DateTime.Now;

            await _stateStore.SetLastPollTimeAsync(lastPoll, cancellationToken);
        }
    }
}