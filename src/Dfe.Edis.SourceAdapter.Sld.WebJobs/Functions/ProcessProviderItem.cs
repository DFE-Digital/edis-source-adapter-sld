using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Application;
using Dfe.Edis.SourceAdapter.Sld.Domain.Queuing;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.WebJobs.Functions
{
    public class ProcessProviderItem
    {
        private readonly IChangeProcessor _changeProcessor;
        private readonly ILogger<ProcessProviderItem> _logger;

        public ProcessProviderItem(
            IChangeProcessor changeProcessor,
            ILogger<ProcessProviderItem> logger)
        {
            _changeProcessor = changeProcessor;
            _logger = logger;
        }
        
        [FunctionName(nameof(ProcessProviderItem))]
        [StorageAccount("Queuing:QueueConnectionString")]
        public async Task RunAsync(
            [QueueTrigger(QueueNames.ProviderQueueName)]
            CloudQueueMessage message,
            CancellationToken cancellationToken)
        {
            using (_logger.BeginScope(new Dictionary<string, object>()
            {
                {"RequestId", Guid.NewGuid().ToString()},
                {"MessageId", message.Id},
                {"DequeueCount", message.DequeueCount},
            }))
            {
                _logger.LogInformation($"Starting to process message {message.Id} on attempt {message.DequeueCount}");

                var queueItem = JsonSerializer.Deserialize<ProviderQueueItem>(message.AsString);
                _logger.LogInformation("Message is for provider {UKPRN} in academic year {AcademicYear}",
                    queueItem.Ukprn, queueItem.AcademicYear);
                
                _logger.LogDebug("Starting change processing...");
                await _changeProcessor.ProcessProviderAsync(queueItem.AcademicYear, queueItem.Ukprn, cancellationToken);
                _logger.LogDebug("Finished change processing...");
            }
        }
    }
}