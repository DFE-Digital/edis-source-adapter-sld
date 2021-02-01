using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.Queuing
{
    public class StorageLearnerQueue : ILearnerQueue
    {
        private CloudQueue _queue;

        public StorageLearnerQueue(
            QueuingConfiguration configuration,
            ILogger<StorageProviderQueue> logger)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.QueueConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            _queue = queueClient.GetQueueReference(QueueNames.LearnerQueueName);
        }
        
        public async Task EnqueueAsync(LearnerQueueItem item, CancellationToken cancellationToken)
        {
            await _queue.AddMessageAsync(new CloudQueueMessage(JsonSerializer.Serialize(item)), cancellationToken);
        }
    }
}