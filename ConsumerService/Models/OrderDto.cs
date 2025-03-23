using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerService.Models
{
    public class OrderDto
    {
        public string OrderId { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
