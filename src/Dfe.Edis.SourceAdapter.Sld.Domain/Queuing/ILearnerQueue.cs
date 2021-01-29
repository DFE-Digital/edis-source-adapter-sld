using System.Threading;
using System.Threading.Tasks;

namespace Dfe.Edis.SourceAdapter.Sld.Domain.Queuing
{
    public interface ILearnerQueue
    {
        Task EnqueueAsync(LearnerQueueItem item, CancellationToken cancellationToken);
    }
}