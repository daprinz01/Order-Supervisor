using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageBroker.Models
{
    public class Order
    {
        public long OrderId { get; set; }
        [Required]
        [Range(1, 10, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int RandomNumber { get; set; }
        [Required]
        public string OrderText { get; set; }
    }
}
