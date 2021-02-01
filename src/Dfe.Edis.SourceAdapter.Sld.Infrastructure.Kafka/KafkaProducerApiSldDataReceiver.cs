using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.Kafka.Producer;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.DataServicesPlatform;
using Dfe.Edis.SourceAdapter.Sld.Domain.SubmitLearnerData;
using Microsoft.Extensions.Logging;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.Kafka
{
    public class KafkaProducerApiSldDataReceiver : ISldDataReceiver
    {
        private readonly IKafkaProducer<string, Learner> _producer;
        private readonly DataServicePlatformConfiguration _configuration;
        private readonly ILogger<KafkaProducerApiSldDataReceiver> _logger;

        public KafkaProducerApiSldDataReceiver(
            IKafkaProducer<string, Learner> producer,
            DataServicePlatformConfiguration configuration,
            ILogger<KafkaProducerApiSldDataReceiver> logger)
        {
            _producer = producer;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendLearnerAsync(Learner learner, CancellationToken cancellationToken)
        {
            await _producer.ProduceAsync(
                _configuration.SldLearnerTopic,
                $"{learner.Ukprn}-{learner.Uln}",
                learner,
                cancellationToken);
        }
    }
}