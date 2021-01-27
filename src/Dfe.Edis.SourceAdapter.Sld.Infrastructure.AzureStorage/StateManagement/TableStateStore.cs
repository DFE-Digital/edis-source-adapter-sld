using System;
using System.Threading;
using System.Threading.Tasks;
using Dfe.Edis.SourceAdapter.Sld.Domain.Configuration;
using Dfe.Edis.SourceAdapter.Sld.Domain.StateManagement;
using Microsoft.Azure.Cosmos.Table;

namespace Dfe.Edis.SourceAdapter.Sld.Infrastructure.AzureStorage.StateManagement
{
    public class TableStateStore : IStateStore
    {
        private CloudTable _table;

        public TableStateStore(
            StateConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.TableConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(configuration.TableName);
        }

        public async Task<DateTime?> GetLastPollTimeAsync(CancellationToken cancellationToken)
        {
            var entity = await RetrieveAsync<PollStateEntity>(PollStateEntity.DefaultPartitionKey, PollStateEntity.LastProviderPollRowKey, cancellationToken);
            return entity?.LastPoll;
        }

        public async Task SetLastPollTimeAsync(DateTime lastPoll, CancellationToken cancellationToken)
        {
            var entity = new PollStateEntity
            {
                RowKey = PollStateEntity.LastProviderPollRowKey,
                LastPoll = lastPoll,
            };
            await InsertOrUpdateAsync(entity, cancellationToken);
        }


        private async Task<T> RetrieveAsync<T>(string partitionKey, string rowKey, CancellationToken cancellationToken)
            where T : TableEntity
        {
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var operationResult = await _table.ExecuteAsync(operation, cancellationToken);
            return (T) operationResult.Result;
        }

        private async Task InsertOrUpdateAsync(TableEntity entity, CancellationToken cancellationToken)
        {
            var operation = TableOperation.InsertOrReplace(entity);
            await _table.ExecuteAsync(operation, cancellationToken);
        }
    }
}