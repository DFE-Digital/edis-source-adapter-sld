namespace Dfe.Edis.SourceAdapter.Sld.Domain.Configuration
{
    public class DataServicePlatformConfiguration
    {
        public string KafkaBootstrapServers { get; set; }
        public string SchemaRegistryUrl { get; set; }
        public string SldLearnerTopic { get; set; }
    }
}