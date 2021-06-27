using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StorageBroker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Implementation
{
    public class TableBroker: ITableBroker
    {
        private readonly ILogger<TableBroker> _logger;
        private readonly IConfiguration _configuration;
        public TableBroker(ILogger<TableBroker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the appsetting file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                _logger.LogError("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the appsettings file - then restart the sample.");

                throw;
            }

            return storageAccount;
        }
        public async Task<CloudTable> CreateTableIfNotExistsAsync(string tableName)
        {
            string storageConnectionString = "UseDevelopmentStorage=true";

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            _logger.LogDebug("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                _logger.LogDebug("Created Table named: {0}", tableName);
            }
            else
            {
                _logger.LogDebug("Table {0} already exists", tableName);
            }
            return table;
        }

        public async Task<T> InsertOrMergeEntityAsync<T>(CloudTable table, T entity) where T : ITableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                T insertedCustomer = (T)result.Result;

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogDebug("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<T> RetrieveEntityUsingPointQueryAsync<T>(CloudTable table, string partitionKey, string rowKey) where T : ITableEntity
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                T customer = (T)result.Result;
                if (customer != null)
                {
                    _logger.LogDebug("Retrieved entity with PartitionKey: \t{0} and RowKey: \t{1}", customer.PartitionKey, customer.RowKey);
                }

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogDebug("Request Charge of Retrieve Operation: " + result.RequestCharge);
                }

                return customer;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task DeleteEntityAsync<T>(CloudTable table, T deleteEntity) where T : ITableEntity
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

                if (result.RequestCharge.HasValue)
                {
                    _logger.LogDebug("Request Charge of Delete Operation: " + result.RequestCharge);
                }

            }
            catch (StorageException e)
            {
                _logger.LogDebug(e.Message);
                throw;
            }
        }
    }
}
