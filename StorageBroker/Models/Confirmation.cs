using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Models
{
    public class Confirmation: TableEntity
    {
        public long OrderId { get; set; }
        public Guid AgentId { get; set; }
        public string OrderStatus { get; set; }
        public Confirmation()
        {

        }
        public Confirmation(string orderId)
        {
            RowKey = orderId;
            PartitionKey = $"1";
        }
    }
}
