using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using StorageBroker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Implementation
{
    public class QueueBroker: IQueueBroker
    {
        private readonly ILogger<QueueBroker> _logger;
        public int orderId = 0;
        public QueueBroker(ILogger<QueueBroker> logger)
        {
            _logger = logger;
        }
        public int GetNextOrderId()
        {
            var result = orderId;
            orderId++;
            return result;
        }
        private async Task<QueueClient> CreateQueueClient(string queueName)
        {
            // Get a reference to a queue and then create it
            QueueClient queue = new QueueClient("UseDevelopmentStorage=true", queueName);
            await queue.CreateIfNotExistsAsync();
            
            return queue;
            
        }
        public async Task AddMessageAsync(string message, string queueName)
        {
            var queue = await CreateQueueClient(queueName);
            // Send a message to order queue
            queue.SendMessage(message);


        }

        public async Task<List<QueueMessage>> ReadMessageAsync(int count, string queueName)
        {
            try
            {
                var queue = await CreateQueueClient(queueName);
                // Get the next messages from the queue
                List<QueueMessage> messages = queue.ReceiveMessages(maxMessages: count).Value.ToList();
                return messages;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}");
                throw;
            }
            
        }
        public async Task DeleteMessage(QueueMessage message, string queueName)
        {
            try
            {
                var queue = await CreateQueueClient(queueName);
                await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception: {exception.Message}");
                throw;
            }
            
        }
    }
}
