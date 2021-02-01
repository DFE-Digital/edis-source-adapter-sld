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
    public class StorageProviderQueue : IProviderQueue
    {
        private CloudQueue _queue;

        public StorageProviderQueue(
            QueuingConfiguration configuration,
            ILogger<StorageProviderQueue> logger)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.QueueConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            _queue = queueClient.GetQueueReference(QueueNames.ProviderQueueName);
        }
        
        public async Task EnqueueAsync(ProviderQueueItem item, CancellationToken cancellationToken)
        {
            await _queue.AddMessageAsync(new CloudQueueMessage(JsonSerializer.Serialize(item)), cancellationToken);
        }
    }
}