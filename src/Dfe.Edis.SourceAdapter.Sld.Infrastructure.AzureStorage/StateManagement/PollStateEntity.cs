using System;
using Microsoft.Azure.Cosmos.Table;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.StateManagement
{
    public class PollStateEntity : TableEntity
    {
        public const string DefaultPartitionKey = "polling";
        public const string LastProviderPollRowKey = "last-provider-poll";
        
        public PollStateEntity()
        {
            PartitionKey = DefaultPartitionKey;
        }
        
        public DateTime LastPoll { get; set; }
    }
}