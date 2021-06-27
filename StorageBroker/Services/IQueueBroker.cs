using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Services
{
    public interface IQueueBroker
    {
        int GetNextOrderId();
        Task AddMessageAsync(string message, string queueName);
        Task<List<QueueMessage>> ReadMessageAsync(int count, string queueName);
        Task DeleteMessage(QueueMessage message, string queueName);
    }
}
