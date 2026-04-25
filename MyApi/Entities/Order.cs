using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyApi.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Status { get; set; } = "Pending";
    }
}