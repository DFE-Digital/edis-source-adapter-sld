using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InMemoryStateStore : IStateStore
    {
        private DateTime? _lastPoll;
        private ConcurrentDictionary<string, ProviderState> _providerState = new ConcurrentDictionary<string, ProviderState>();

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

        public Task<ProviderState> GetProviderStateAsync(string academicYear, int provider, CancellationToken cancellationToken)
        {
            var key = $"{academicYear}|{provider}".ToLower();
            _providerState.TryGetValue(key, out var state);
            return Task.FromResult(state);
        }

        public Task SetProviderStateAsync(ProviderState state, CancellationToken cancellationToken)
        {
            var key = $"{state.AcademicYear}|{state.Ukprn}".ToLower();
            _providerState.AddOrUpdate(key, state, (existingKey, existingState) => state);
            return Task.CompletedTask;
        }
    }
}