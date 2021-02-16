using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions;
using Microsoft.Azure.Storage.Queue;

namespace Dfe.Edis.SourceAdapter.Sld.AcceptanceTests.TestHarness
{
    public class InProcessProviderQueue : IProviderQueue
    {
        private readonly Func<Type, object> _factory;
        private Queue<ProviderQueueItem> _queue = new Queue<ProviderQueueItem>();

        public InProcessProviderQueue(Func<Type, object> factory)
        {
            _factory = factory;
        }
        
        public Task EnqueueAsync(ProviderQueueItem item, CancellationToken cancellationToken)
        {
            _queue.Enqueue(item);
            return Task.CompletedTask;
        }

        public void Reset()
        {
            _queue.Clear();
        }

        public async Task DrainAsync()
        {
            var function = (ProcessProviderItem)_factory(typeof(ProcessProviderItem));
            while (_queue.Count > 0)
            {
                var nextItem = _queue.Dequeue();
            
                var message = new CloudQueueMessage(JsonSerializer.Serialize(nextItem));
                await function.RunAsync(message, CancellationToken.None);
            }
        }
    }
}