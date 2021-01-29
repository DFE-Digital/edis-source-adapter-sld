using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement
{
    public interface IStateStore
    {
        Task<DateTime?> GetLastPollTimeAsync(CancellationToken cancellationToken);
        Task SetLastPollTimeAsync(DateTime lastPoll, CancellationToken cancellationToken);

        Task<ProviderState> GetProviderStateAsync(string academicYear, int provider, CancellationToken cancellationToken);
        Task SetProviderStateAsync(ProviderState state, CancellationToken cancellationToken);
    }
}