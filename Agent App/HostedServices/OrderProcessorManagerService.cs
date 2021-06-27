using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StorageBroker.Models;
using StorageBroker.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agent_App.HostedServices
{
    public class OrderProcessorManagerService : BackgroundService
    {
        private readonly ILogger<OrderProcessorManagerService> _logger;
        private readonly IQueueBroker _queueBroker;
        private readonly ITableBroker _tableBroker;
        private readonly IUtilities _utilities;
        private const string _ordersQueueName = "orders";

        public OrderProcessorManagerService(ILogger<OrderProcessorManagerService> logger, 
                                         IQueueBroker queueBroker, 
                                         ITableBroker tableBroker, 
                                         IUtilities utilities)
        {
            _logger = logger;
            _queueBroker = queueBroker;
            _tableBroker = tableBroker;
            _utilities = utilities;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"Agent job order processor is starting.");

            stoppingToken.Register(() =>
                _logger.LogDebug($"Agent job order processor task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Agent job order processor task doing background work.");

                await ProcessOrdersAsync(stoppingToken);

                await Task.Delay(1, stoppingToken);
            }

            _logger.LogDebug($"Agent job order processor task is stopping.");
        }

        async Task ProcessOrdersAsync(CancellationToken stoppingToken)
        {
            var agentId = Guid.NewGuid();
            var magicNumber = _utilities.GenerateMagicNumber();
            _logger.LogInformation($"“I’m agent {agentId}, my magic number is {magicNumber}");
           

            while (true)
            {
                var ordersQueue = await _queueBroker.ReadMessageAsync(1, _ordersQueueName);
                foreach (var item in ordersQueue)
                {
                    var order = JsonConvert.DeserializeObject<Order>($"{item.Body}");
                    _logger.LogInformation($"Received order {order.OrderId}");
                    if (order.RandomNumber == magicNumber)
                    {
                        _logger.LogInformation("Oh no, my magic number was found");
                        // I am not entirely sure if I am to stop the application and wait for a key to be enter to close it or wait for a key to be entered to reset the magic number
                        await StopAsync(stoppingToken);
                    }
                    else
                    {
                        _logger.LogInformation($"Order Text: {order.OrderText}");
                        var confirmationsTable = await _tableBroker.CreateTableIfNotExistsAsync("confirmations");
                        _ = await _tableBroker.InsertOrMergeEntityAsync(confirmationsTable, new Confirmation($"{order.OrderId}")
                        {
                            AgentId = agentId,
                            OrderId = order.OrderId,
                            OrderStatus = "Processed",
                        });
                       await _queueBroker.DeleteMessage(item, _ordersQueueName);
                    }
                }
            }
        }


    }
}
