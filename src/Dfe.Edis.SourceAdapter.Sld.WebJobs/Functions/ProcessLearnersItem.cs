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
    public class ProcessLearnersItem
    {
        private readonly IChangeProcessor _changeProcessor;
        private readonly ILogger<ProcessLearnersItem> _logger;

        public ProcessLearnersItem(
            IChangeProcessor changeProcessor,
            ILogger<ProcessLearnersItem> logger)
        {
            _changeProcessor = changeProcessor;
            _logger = logger;
        }
        
        [FunctionName(nameof(ProcessLearnersItem))]
        [StorageAccount("Queuing:QueueConnectionString")]
        public async Task RunAsync(
            [QueueTrigger(QueueNames.LearnerQueueName)]
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

                var queueItem = JsonSerializer.Deserialize<LearnerQueueItem>(message.AsString);
                _logger.LogInformation("Message is for page {PageNumber} of provider {UKPRN} in academic year {AcademicYear}",
                    queueItem.PageNumber, queueItem.Ukprn, queueItem.AcademicYear);
                
                _logger.LogDebug("Starting change processing...");
                await _changeProcessor.ProcessLearnerAsync(queueItem.AcademicYear, queueItem.Ukprn, queueItem.PageNumber, cancellationToken);
                _logger.LogDebug("Finished change processing...");
            }
        }
    }
}