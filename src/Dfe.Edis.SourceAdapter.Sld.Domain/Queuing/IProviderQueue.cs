using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.Queuing
{
    public interface IProviderQueue
    {
        Task EnqueueAsync(ProviderQueueItem item, CancellationToken cancellationToken);
    }
}