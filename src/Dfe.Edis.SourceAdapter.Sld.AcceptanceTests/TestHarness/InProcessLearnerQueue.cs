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
    public class InProcessLearnerQueue : ILearnerQueue
    {
        private readonly Func<Type, object> _factory;
        private Queue<LearnerQueueItem> _queue = new Queue<LearnerQueueItem>();

        public InProcessLearnerQueue(Func<Type, object> factory)
        {
            _factory = factory;
        }
        public Task EnqueueAsync(LearnerQueueItem item, CancellationToken cancellationToken)
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
            var function = (ProcessLearnersItem)_factory(typeof(ProcessLearnersItem));
            while (_queue.Count > 0)
            {
                var nextItem = _queue.Dequeue();
            
                var message = new CloudQueueMessage(JsonSerializer.Serialize(nextItem));
                await function.RunAsync(message, CancellationToken.None);
            }
        }
    }
}