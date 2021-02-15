using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InMemoryStateStore : IStateStore
    {
        private DateTime? _lastPoll;

        public void Reset()
        {
            _lastPoll = null;
        }
        
        public Task<DateTime?> GetLastPollTimeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_lastPoll);
        }

        public Task SetLastPollTimeAsync(DateTime lastPoll, CancellationToken cancellationToken)
        {
            _lastPoll = lastPoll;
            return Task.CompletedTask;
        }

        public async Task<ProviderState> GetProviderStateAsync(string academicYear, int provider, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task SetProviderStateAsync(ProviderState state, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}