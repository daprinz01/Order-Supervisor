using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Services
{
    public interface ITableBroker
    {
        Task<CloudTable> CreateTableIfNotExistsAsync(string tableName);
        Task<T> InsertOrMergeEntityAsync<T>(CloudTable table, T entity) where T : ITableEntity;
        Task<T> RetrieveEntityUsingPointQueryAsync<T>(CloudTable table, string partitionKey, string rowKey) where T : ITableEntity;
        Task DeleteEntityAsync<T>(CloudTable table, T deleteEntity) where T : ITableEntity;
    }
}
