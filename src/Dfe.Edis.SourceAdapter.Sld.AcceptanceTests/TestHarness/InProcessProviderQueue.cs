using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InProcessProviderQueue : IProviderQueue
    {
        public Task EnqueueAsync(ProviderQueueItem item, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}