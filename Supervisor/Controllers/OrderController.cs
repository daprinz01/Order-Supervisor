using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StorageBroker.Models;
using StorageBroker.Services;
using Supervisor.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Supervisor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IQueueBroker _queueBroker;
        private readonly ITableBroker _tableBroker;
        private const string _ordersQueueName = "orders";
        private const string _confirmationsTableName = "confirmations";

        public OrderController(ILogger<OrderController> logger, IQueueBroker queueBroker, ITableBroker tableBroker)
        {
            _logger = logger;
            _queueBroker = queueBroker;
            _tableBroker = tableBroker;
        }
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Confirmation))]
        [ProducesResponseType((int)HttpStatusCode.Conflict, Type = typeof(ErrorResponse))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Confirmation>> Post(Order order)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    order.OrderId = _queueBroker.GetNextOrderId();
                    _logger.LogInformation($"Send order {order.OrderId} with random number RandomNumber");
                    await _queueBroker.AddMessageAsync(JsonConvert.SerializeObject(order), _ordersQueueName);
                    await Task.Delay(100);
                    var confirmationTable = await _tableBroker.CreateTableIfNotExistsAsync(_confirmationsTableName);
                    var confirmation = await _tableBroker.RetrieveEntityUsingPointQueryAsync<Confirmation>(confirmationTable, $"1", $"{order.OrderId}");
                    return Ok(confirmation);
                }
                catch (Exception exception)
                {

                    return Conflict(new ErrorResponse
                    {
                        ResponseCode = "99",
                        ResponseMessage = $"Exception occured: {exception.Message}"
                    });
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(m => m.Errors);
                _logger.LogError($"Validation errors occurred.... {JsonConvert.SerializeObject(errors)}");
                return BadRequest(new ErrorResponse(ModelState));
            }
        }
    }
}
